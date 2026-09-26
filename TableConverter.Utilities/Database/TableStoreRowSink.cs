using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Models.TableStore;

namespace TableConverter.Utilities.Database;

/// <summary>
///     Fills a table store from a row stream, so an importer can hand over rows as it parses them
///     instead of holding the whole table in memory first.
/// </summary>
/// <remarks>
///     <para>
///         The store is written with raw SQL rather than through the change tracker: the entity changed
///         events raised by <c>SaveChanges</c> would add every column to the grid a second time.
///     </para>
///     <para>
///         The whole write happens in one transaction that is opened by <see cref="BeginAsync" /> and
///         committed by <see cref="CompleteAsync" />. Disposing a sink that was never completed rolls
///         the transaction back, so a parse that fails half way through leaves the store as it was
///         rather than half filled.
///     </para>
/// </remarks>
public sealed class TableStoreRowSink : ITableRowSink, IAsyncDisposable
{
    /// <summary>
    ///     The data type stored for every written column. An imported table carries no type information,
    ///     so its columns are text, matching the columns produced by the New File command.
    /// </summary>
    private const int TextDataType = (int)ColumnDataType.Text;

    /// <summary>
    ///     The number of cells buffered before a batch is inserted. Each cell binds one parameter, and
    ///     the default SQLite limit on bound parameters has historically been as low as 999, so the
    ///     batches stay well clear of it.
    /// </summary>
    private const int CellsPerBatch = 500;

    /// <summary>
    ///     The number of rows buffered before the pending row ids are inserted. Cells are what bind
    ///     parameters, so a table with no columns would otherwise never flush its rows.
    /// </summary>
    private const int RowsPerBatch = 500;

    /// <summary>
    ///     The store is expected to be empty when this runs, but clearing makes the write idempotent and
    ///     lets the row and column ids be assigned explicitly, keeping them in step with the written
    ///     table.
    /// </summary>
    private const string ClearStoreSql = """
        DELETE FROM SEARCH_RESULT;
        DELETE FROM CELLS;
        DELETE FROM ROWS;
        DELETE FROM COLUMNS;
        """;

    private readonly TableStoreDbContext _dbContext;

    // Rows and cells are buffered separately because ROWS has to be inserted before the CELLS that
    // reference it. Both buffers are flushed together, rows first.
    private readonly StringBuilder _rowValues = new();
    private readonly StringBuilder _cellValues = new();
    private readonly List<object> _parameters = new(CellsPerBatch);

    private IDbContextTransaction? _transaction;
    private int _columnCount;

    /// <summary>The number of row ids currently waiting in <see cref="_rowValues" />.</summary>
    private int _bufferedRows;

    /// <summary>The total number of rows handed over, from which the row ids are assigned.</summary>
    private int _rowCount;

    private bool _begun;
    private bool _completed;
    private bool _disposed;

    private TableStoreRowSink(TableStoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public bool IsCompleted => _completed;

    /// <summary>
    ///     Creates a sink that writes into the store behind <paramref name="dbContext" />.
    /// </summary>
    /// <param name="dbContext">A context bound to the store to write to. The caller owns it.</param>
    /// <remarks>
    ///     Nothing is written until <see cref="BeginAsync" /> is called, and the sink never disposes the
    ///     context.
    /// </remarks>
    public static TableStoreRowSink Create(TableStoreDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        return new TableStoreRowSink(dbContext);
    }

    /// <inheritdoc />
    public async Task BeginAsync(IReadOnlyList<string> headers, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(headers);
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_begun)
        {
            throw new InvalidOperationException("The sink has already been started.");
        }

        _begun = true;
        _columnCount = headers.Count;

        _transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        await _dbContext.Database
            .ExecuteSqlRawAsync(ClearStoreSql, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        await WriteColumnsAsync(headers, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task WriteRowAsync(IReadOnlyList<string?> cells, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cells);
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_begun)
        {
            throw new InvalidOperationException("The sink has not been started.");
        }

        if (_completed)
        {
            throw new InvalidOperationException("The sink has already been completed.");
        }

        var rowId = ++_rowCount;

        AppendRow(rowId);

        for (var columnIndex = 0; columnIndex < _columnCount; columnIndex++)
        {
            // Short rows are padded rather than dropped, so every row ends up with a cell per column
            // and the grid can always bind by index.
            var value = columnIndex < cells.Count ? cells[columnIndex] : null;
            var parameterName = $"@cell{_parameters.Count}";

            if (_cellValues.Length > 0)
            {
                _cellValues.Append(',');
            }

            _cellValues.Append('(')
                .Append(rowId).Append(',')
                .Append(columnIndex + 1).Append(',')
                .Append(parameterName).Append(')');

            // A missing value is stored as NULL rather than an empty string, so the two stay
            // distinguishable. It travels in a named SqliteParameter because EF Core has no store type
            // mapping for DBNull, which is what a bare null argument would be bound as.
            _parameters.Add(new SqliteParameter(parameterName, (object?)value ?? DBNull.Value));

            if (_parameters.Count >= CellsPerBatch)
            {
                // Safe to flush mid row: the row id is already buffered, so the cells that follow in a
                // later batch still point at a row that exists.
                await FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        if (_bufferedRows >= RowsPerBatch)
        {
            await FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_begun)
        {
            throw new InvalidOperationException("The sink has not been started.");
        }

        if (_completed)
        {
            return;
        }

        await FlushAsync(cancellationToken).ConfigureAwait(false);

        if (_transaction is not null)
        {
            await _transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        _completed = true;
    }

    /// <summary>
    ///     Rolls the write back when <see cref="CompleteAsync" /> was never reached.
    /// </summary>
    /// <remarks>
    ///     The store context is left alone: the caller created it and is responsible for disposing it.
    /// </remarks>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_transaction is null)
        {
            return;
        }

        if (!_completed)
        {
            await _transaction.RollbackAsync().ConfigureAwait(false);
        }

        await _transaction.DisposeAsync().ConfigureAwait(false);
        _transaction = null;
    }

    private void AppendRow(int rowId)
    {
        if (_rowValues.Length > 0)
        {
            _rowValues.Append(',');
        }

        _rowValues.Append('(').Append(rowId).Append(')');
        _bufferedRows++;
    }

    private async Task WriteColumnsAsync(IReadOnlyList<string> headers, CancellationToken cancellationToken)
    {
        for (var index = 0; index < headers.Count; index++)
        {
            var ordinalPosition = index + 1;
            var header = headers[index];

            await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                 INSERT INTO COLUMNS (ID, NAME, DATA_TYPE, ORDINAL_POSITION)
                 VALUES ({ordinalPosition}, {header}, {TextDataType}, {ordinalPosition});
                 """, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task FlushAsync(CancellationToken cancellationToken)
    {
        // ROWS has to land before the CELLS that point at it, which is why the row ids are always
        // inserted first even when only the cell buffer is full.
        if (_rowValues.Length > 0)
        {
            var rows = _rowValues.ToString();

            _rowValues.Clear();
            _bufferedRows = 0;

            // The ids are integers this class chose, so they are safe to inline and save a parameter
            // per row.
#pragma warning disable EF1002 // Only generated integer ids are inlined; nothing user supplied reaches the SQL text.
            await _dbContext.Database
                .ExecuteSqlRawAsync($"INSERT INTO ROWS (ID) VALUES {rows};", cancellationToken: cancellationToken)
                .ConfigureAwait(false);
#pragma warning restore EF1002
        }

        if (_parameters.Count == 0)
        {
            return;
        }

        var cells = _cellValues.ToString();

        _cellValues.Clear();

#pragma warning disable EF1002 // Only generated parameter names and integer ids are inlined; every cell value is bound.
        await _dbContext.Database
            .ExecuteSqlRawAsync($"INSERT INTO CELLS (ROW_ID, COLUMN_ID, VALUE) VALUES {cells};", _parameters,
                cancellationToken)
            .ConfigureAwait(false);
#pragma warning restore EF1002

        _parameters.Clear();
    }
}

