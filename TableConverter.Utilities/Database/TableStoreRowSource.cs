using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using TableConverter.Utilities.Database.Contexts;

namespace TableConverter.Utilities.Database;

/// <summary>
///     Streams the contents of a table store a page at a time, so an exporter can write a large table
///     out without the whole of it being held in memory.
/// </summary>
/// <remarks>
///     Rows are read with keyset paging (<c>WHERE ID &gt; last</c>) rather than with <c>OFFSET</c>, so
///     the cost of reaching a page does not grow with how far into the table it is. Only the cells of
///     the rows in the current page are loaded, which is what keeps the memory a read needs bounded by
///     the page size instead of by the size of the table.
/// </remarks>
public sealed class TableStoreRowSource : ITableRowSource
{
    /// <summary>
    ///     The number of rows loaded per page. Large enough that the per query overhead is amortised,
    ///     small enough that a page is a bounded amount of memory.
    /// </summary>
    public const int DefaultRowsPerPage = 1000;

    private readonly TableStoreDbContext _dbContext;
    private readonly int _rowsPerPage;

    private IReadOnlyList<string>? _headers;
    private Dictionary<int, int>? _indexByColumnId;

    private TableStoreRowSource(TableStoreDbContext dbContext, int rowsPerPage)
    {
        _dbContext = dbContext;
        _rowsPerPage = rowsPerPage;
    }

    /// <summary>
    ///     Creates a source that reads from the store behind <paramref name="dbContext" />.
    /// </summary>
    /// <param name="dbContext">A context bound to the store to read. The caller owns it.</param>
    /// <param name="rowsPerPage">The number of rows to load per page.</param>
    /// <remarks>
    ///     The source never disposes the context.
    /// </remarks>
    public static TableStoreRowSource Create(TableStoreDbContext dbContext, int rowsPerPage = DefaultRowsPerPage)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentOutOfRangeException.ThrowIfLessThan(rowsPerPage, 1);

        return new TableStoreRowSource(dbContext, rowsPerPage);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetHeadersAsync(CancellationToken cancellationToken = default)
    {
        await EnsureColumnsAsync(cancellationToken).ConfigureAwait(false);

        return _headers!;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string?[]> ReadRowsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await EnsureColumnsAsync(cancellationToken).ConfigureAwait(false);

        var columnCount = _headers!.Count;
        var indexByColumnId = _indexByColumnId!;
        var lastRowId = 0;

        while (true)
        {
            var rowIds = await _dbContext.Rows
                .AsNoTracking()
                .Where(row => row.Id > lastRowId)
                .OrderBy(row => row.Id)
                .Select(row => row.Id)
                .Take(_rowsPerPage)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (rowIds.Count == 0)
            {
                yield break;
            }

            var firstRowId = rowIds[0];
            var lastRowIdInPage = rowIds[^1];

            // Only the cells of this page are pulled back, which is the whole point of paging: a table
            // of any size costs one page of memory to read.
            var cellsByRowId = (await _dbContext.Cells
                    .AsNoTracking()
                    .Where(cell => cell.RowId >= firstRowId && cell.RowId <= lastRowIdInPage)
                    .Select(cell => new CellValue(cell.RowId, cell.ColumnId, cell.Value))
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false))
                .ToLookup(cell => cell.RowId);

            foreach (var rowId in rowIds)
            {
                var row = new string?[columnCount];

                foreach (var cell in cellsByRowId[rowId])
                {
                    // A cell is positioned by the ordinal of its column rather than by its id, because
                    // a column can be deleted and re-added, which leaves gaps in the ids.
                    if (indexByColumnId.TryGetValue(cell.ColumnId, out var index))
                    {
                        row[index] = cell.Value;
                    }
                }

                yield return row;
            }

            lastRowId = lastRowIdInPage;
        }
    }

    private async Task EnsureColumnsAsync(CancellationToken cancellationToken)
    {
        if (_headers is not null)
        {
            return;
        }

        var columns = await _dbContext.Columns
            .AsNoTracking()
            .OrderBy(column => column.OrdinalPosition)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var indexByColumnId = new Dictionary<int, int>(columns.Count);
        var headers = new string[columns.Count];

        for (var index = 0; index < columns.Count; index++)
        {
            headers[index] = columns[index].Name;
            indexByColumnId[columns[index].Id] = index;
        }

        _indexByColumnId = indexByColumnId;
        _headers = headers;
    }

    private readonly record struct CellValue(int RowId, int ColumnId, string? Value);
}

