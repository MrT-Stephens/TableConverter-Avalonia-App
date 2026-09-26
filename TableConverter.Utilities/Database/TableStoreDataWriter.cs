using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Models.TableStore;

namespace TableConverter.Utilities.Database;

/// <summary>
/// Replaces the contents of a table store with a <see cref="TableData" />.
/// </summary>
/// <remarks>
/// The table is written with raw SQL rather than through the change tracker: the entity changed
/// events raised by <c>SaveChanges</c> would add every column to the grid a second time.
/// </remarks>
public static class TableStoreDataWriter
{
    /// <summary>
    /// The data type stored for every written column. An imported table carries no type information, so
    /// its columns are text, matching the columns produced by the New File command.
    /// </summary>
    private const int TextDataType = (int)ColumnDataType.Text;

    /// <summary>
    /// The number of cells written per insert. Each cell binds one parameter, and the default SQLite
    /// limit on bound parameters has historically been as low as 999, so the batches stay well clear
    /// of it.
    /// </summary>
    private const int CellsPerBatch = 500;

    /// <summary>
    /// The store is expected to be empty when this runs, but clearing makes the write idempotent and
    /// lets the row and column ids be assigned explicitly, keeping them in step with the written
    /// table.
    /// </summary>
    private const string ClearStoreSql = """
        DELETE FROM SEARCH_RESULT;
        DELETE FROM CELLS;
        DELETE FROM ROWS;
        DELETE FROM COLUMNS;
        """;

    /// <summary>
    /// Replaces everything in the store behind <paramref name="dbContext" /> with
    /// <paramref name="tableData" />.
    /// </summary>
    /// <param name="dbContext">A context bound to the store to write to.</param>
    /// <param name="tableData">The table to store.</param>
    /// <param name="cancellationToken">Token used to cancel the write.</param>
    /// <remarks>
    /// The whole write happens in one transaction, so a failure part way through leaves the store as
    /// it was rather than half emptied.
    /// </remarks>
    public static async Task WriteAsync(
        TableStoreDbContext dbContext,
        TableData tableData,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(tableData);

        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            await dbContext.Database
                .ExecuteSqlRawAsync(ClearStoreSql, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            await WriteColumnsAsync(dbContext, tableData.Headers, cancellationToken).ConfigureAwait(false);
            await WriteRowsAsync(dbContext, tableData.Rows.Count, cancellationToken).ConfigureAwait(false);
            await WriteCellsAsync(dbContext, tableData, cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    private static async Task WriteColumnsAsync(
        TableStoreDbContext dbContext,
        IReadOnlyList<string> headers,
        CancellationToken cancellationToken)
    {
        for (var index = 0; index < headers.Count; index++)
        {
            var ordinalPosition = index + 1;
            var header = headers[index];

            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                 INSERT INTO COLUMNS (ID, NAME, DATA_TYPE, ORDINAL_POSITION)
                 VALUES ({ordinalPosition}, {header}, {TextDataType}, {ordinalPosition});
                 """, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task WriteRowsAsync(
        TableStoreDbContext dbContext,
        int rowCount,
        CancellationToken cancellationToken)
    {
        if (rowCount == 0)
        {
            return;
        }

        // The ids are integers this method chose, so they are safe to inline and save a parameter per
        // row.
        var rows = string.Join(',', Enumerable.Range(1, rowCount).Select(id => $"({id})"));

#pragma warning disable EF1002 // Only generated integer ids are inlined; nothing user supplied reaches the SQL text.
        await dbContext.Database
            .ExecuteSqlRawAsync($"INSERT INTO ROWS (ID) VALUES {rows};", cancellationToken: cancellationToken)
            .ConfigureAwait(false);
#pragma warning restore EF1002
    }

    private static async Task WriteCellsAsync(
        TableStoreDbContext dbContext,
        TableData tableData,
        CancellationToken cancellationToken)
    {
        var columnCount = tableData.Headers.Count;

        if (columnCount == 0 || tableData.Rows.Count == 0)
        {
            return;
        }

        var values = new StringBuilder();
        var parameters = new List<object>(CellsPerBatch);

        for (var rowIndex = 0; rowIndex < tableData.Rows.Count; rowIndex++)
        {
            var row = tableData.Rows[rowIndex];

            for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                // Short rows are padded rather than dropped, so every row ends up with a cell per
                // column and the grid can always bind by index.
                var value = columnIndex < row.Length ? row[columnIndex] : null;
                var parameterName = $"@cell{parameters.Count}";

                if (values.Length > 0)
                {
                    values.Append(',');
                }

                values.Append('(')
                    .Append(rowIndex + 1).Append(',')
                    .Append(columnIndex + 1).Append(',')
                    .Append(parameterName).Append(')');

                // A missing value is stored as NULL rather than an empty string, so the two stay
                // distinguishable. It travels in a named SqliteParameter because EF Core has no store
                // type mapping for DBNull, which is what a bare null argument would be bound as.
                parameters.Add(new SqliteParameter(parameterName, (object?)value ?? DBNull.Value));

                if (parameters.Count >= CellsPerBatch)
                {
                    await FlushCellsAsync(dbContext, values, parameters, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        if (parameters.Count > 0)
        {
            await FlushCellsAsync(dbContext, values, parameters, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task FlushCellsAsync(
        TableStoreDbContext dbContext,
        StringBuilder values,
        List<object> parameters,
        CancellationToken cancellationToken)
    {
        await dbContext.Database
#pragma warning disable EF1002 // Only generated parameter names and integer ids are inlined; every cell value is bound.
            .ExecuteSqlRawAsync($"INSERT INTO CELLS (ROW_ID, COLUMN_ID, VALUE) VALUES {values};", parameters,
                cancellationToken)
#pragma warning restore EF1002
            .ConfigureAwait(false);

        values.Clear();
        parameters.Clear();
    }
}
