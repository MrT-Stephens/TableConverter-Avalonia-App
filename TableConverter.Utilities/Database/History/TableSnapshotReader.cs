using Microsoft.EntityFrameworkCore;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Models;

namespace TableConverter.Utilities.Database.History;

/// <summary>
///     The value a cell held, at the place it was held in.
/// </summary>
public sealed record CellValue(int RowId, int ColumnId, string? Value);

/// <summary>
///     Every value a row held, keyed by the column the value belongs to.
/// </summary>
public sealed record RowValues(int RowId, Dictionary<int, string?> Cells);

/// <summary>
///     A column and what it was, which is everything needed to put the column back exactly as it was.
/// </summary>
/// <param name="Cells">
///     The values the column's cells held, or <see langword="null" /> when the caller only wanted what the
///     column is rather than what it holds. A column that is only being renamed carries no values, because
///     remembering every value of a wide table in order to record a rename would cost far more than the
///     rename is worth.
/// </param>
public sealed record ColumnValues(
    int ColumnId,
    string Name,
    ColumnDataType DataType,
    string? DefaultValueForCell,
    int OrdinalPosition,
    Dictionary<int, string?>? Cells);

/// <summary>
///     A whole table, held as the columns it has and the rows it holds.
/// </summary>
public sealed record TableSnapshot(List<ColumnValues> Columns, List<RowValues> Rows);

/// <summary>
///     Reads the parts of a store that a history entry needs in order to be able to put them back.
/// </summary>
/// <remarks>
///     A reader takes the narrowest thing it can: an entry remembers the values it is about to change
///     rather than the table around them, because a store is the size it is precisely because its table
///     is large. Only an operation that rewrites the whole table reads the whole of it.
/// </remarks>
internal static class TableSnapshotReader
{
    /// <summary>
    ///     How many ids are named in one statement. A statement can only take so many parameters, and an
    ///     operation can touch far more rows than that.
    /// </summary>
    private const int RowsPerBatch = 500;

    /// <summary>
    ///     Reads every value of the named rows, keyed by row and column.
    /// </summary>
    public static async Task<Dictionary<(int RowId, int ColumnId), string?>> ReadCellsByRowAsync(
        TableStoreDbContext db,
        IEnumerable<int> rowIds,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<(int RowId, int ColumnId), string?>();

        foreach (var batch in rowIds.Distinct().Chunk(RowsPerBatch))
        {
            var ids = batch.ToArray();

            var cells = await db.Cells
                .AsNoTracking()
                .Where(cell => ids.Contains(cell.RowId))
                .Select(cell => new { cell.RowId, cell.ColumnId, cell.Value })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var cell in cells)
            {
                result[(cell.RowId, cell.ColumnId)] = cell.Value;
            }
        }

        return result;
    }

    /// <summary>
    ///     Reads the named rows, each with every value it holds.
    /// </summary>
    /// <remarks>
    ///     Only the rows the table actually holds come back. A row that is not there is what an operation
    ///     that deletes one leaves behind, so a reader that invented a row for every id it was given could
    ///     not tell a row that was removed from a row that was emptied.
    /// </remarks>
    public static async Task<List<RowValues>> ReadRowsAsync(
        TableStoreDbContext db,
        IReadOnlyCollection<int> rowIds,
        CancellationToken cancellationToken = default)
    {
        var values = await ReadCellsByRowAsync(db, rowIds, cancellationToken).ConfigureAwait(false);

        var cellsByRow = values
            .GroupBy(pair => pair.Key.RowId)
            .ToDictionary(
                group => group.Key,
                group => group.ToDictionary(pair => pair.Key.ColumnId, pair => pair.Value));

        var existing = new HashSet<int>();

        foreach (var batch in rowIds.Distinct().Chunk(RowsPerBatch))
        {
            var ids = batch.ToArray();

            var found = await db.Rows
                .AsNoTracking()
                .Where(row => ids.Contains(row.Id))
                .Select(row => row.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var id in found)
            {
                existing.Add(id);
            }
        }

        return
        [
            .. existing
                .Order()
                .Select(rowId => new RowValues(
                    rowId,
                    cellsByRow.TryGetValue(rowId, out var cells) ? cells : []))
        ];
    }

    /// <summary>
    ///     Reads the named cells.
    /// </summary>
    /// <remarks>
    ///     The values are looked up a row at a time rather than a cell at a time, because the cells a
    ///     search turned up are scattered over the table and asking for each of them on its own would be
    ///     a statement per value.
    /// </remarks>
    public static async Task<List<CellValue>> ReadCellsAsync(
        TableStoreDbContext db,
        IReadOnlyCollection<(int RowId, int ColumnId)> keys,
        CancellationToken cancellationToken = default)
    {
        var wanted = keys.ToHashSet();

        if (wanted.Count == 0)
        {
            return [];
        }

        var values = await ReadCellsByRowAsync(db, wanted.Select(key => key.RowId), cancellationToken)
            .ConfigureAwait(false);

        return
        [
            .. wanted
                .OrderBy(key => key.RowId)
                .ThenBy(key => key.ColumnId)
                .Where(key => values.ContainsKey(key))
                .Select(key => new CellValue(key.RowId, key.ColumnId, values[key]))
        ];
    }

    /// <summary>
    ///     Reads what the named columns are.
    /// </summary>
    /// <param name="db">The store to read from.</param>
    /// <param name="columnIds">The columns to read.</param>
    /// <param name="withCellIds">
    ///     The columns whose values are wanted as well, or <see langword="null" /> when only what the
    ///     columns are is wanted.
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the read.</param>
    public static async Task<List<ColumnValues>> ReadColumnsAsync(
        TableStoreDbContext db,
        IReadOnlyCollection<int> columnIds,
        IReadOnlyCollection<int>? withCellIds = null,
        CancellationToken cancellationToken = default)
    {
        var wanted = columnIds.ToHashSet();

        if (wanted.Count == 0)
        {
            return [];
        }

        var columns = await db.Columns
            .AsNoTracking()
            .Where(column => wanted.Contains(column.Id))
            .OrderBy(column => column.OrdinalPosition)
            .ThenBy(column => column.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var cellsByColumn = new Dictionary<int, Dictionary<int, string?>>();

        // An empty set means the caller wants to know what the columns are and not what they hold, which
        // is a different thing from wanting to know that they hold nothing.
        var captureCells = withCellIds is { Count: > 0 };

        // Only the columns that were named have their values read. A column that was not named keeps a
        // null set rather than an empty one, because the two say different things: null is a column whose
        // values were not asked about and must be left alone, while an empty set is a column that was
        // asked about and holds nothing, whose values are to be cleared.
        var wantedCellColumns = captureCells ? withCellIds!.ToHashSet() : null;

        if (captureCells)
        {
            foreach (var batch in withCellIds!.Distinct().Chunk(RowsPerBatch))
            {
                var ids = batch;

                var cells = await db.Cells
                    .AsNoTracking()
                    .Where(cell => ids.Contains(cell.ColumnId))
                    .Select(cell => new { cell.RowId, cell.ColumnId, cell.Value })
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                foreach (var cell in cells)
                {
                    if (!cellsByColumn.TryGetValue(cell.ColumnId, out var columnCells))
                    {
                        columnCells = [];
                        cellsByColumn[cell.ColumnId] = columnCells;
                    }

                    columnCells[cell.RowId] = cell.Value;
                }
            }
        }

        return
        [
            .. columns.Select(column =>
            {
                var cells = wantedCellColumns?.Contains(column.Id) == true
                    ? cellsByColumn.TryGetValue(column.Id, out var captured) ? captured : []
                    : null;

                return new ColumnValues(
                    column.Id,
                    column.Name,
                    column.DataType,
                    column.DefaultValueForCell,
                    column.OrdinalPosition,
                    cells);
            })
        ];
    }

    /// <summary>
    ///     Reads the ids of every row of the table, in the order the table keeps them.
    /// </summary>
    public static async Task<List<int>> ReadRowOrderAsync(
        TableStoreDbContext db,
        CancellationToken cancellationToken = default)
    {
        return await db.Rows
            .AsNoTracking()
            .OrderBy(row => row.Id)
            .Select(row => row.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     Reads the whole of the table. Only an operation that replaces the table needs this.
    /// </summary>
    public static async Task<TableSnapshot> ReadTableAsync(
        TableStoreDbContext db,
        CancellationToken cancellationToken = default)
    {
        var columnIds = await db.Columns
            .AsNoTracking()
            .Select(column => column.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var columns = await ReadColumnsAsync(db, columnIds, columnIds, cancellationToken).ConfigureAwait(false);

        var rowIds = await ReadRowOrderAsync(db, cancellationToken).ConfigureAwait(false);
        var rows = await ReadRowsAsync(db, rowIds, cancellationToken).ConfigureAwait(false);

        return new TableSnapshot(columns, rows);
    }
}
