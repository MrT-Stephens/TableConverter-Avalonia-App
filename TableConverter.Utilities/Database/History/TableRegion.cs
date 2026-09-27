using Microsoft.EntityFrameworkCore;
using TableConverter.Utilities.Database.Contexts;

namespace TableConverter.Utilities.Database.History;

/// <summary>
///     The part of a table an operation is about to change.
/// </summary>
public enum TableRegionKind
{
    /// <summary>Whole rows, with the values they hold.</summary>
    Rows,

    /// <summary>Named cells, with the values they hold.</summary>
    Cells,

    /// <summary>What the columns of the table are.</summary>
    Columns,

    /// <summary>The order the table keeps its rows in.</summary>
    RowOrder,

    /// <summary>A quarter turn of the whole table.</summary>
    Rotation,

    /// <summary>The whole of the table.</summary>
    Table,
}

/// <summary>
///     Says which part of a table an operation is about to change, so the history can remember it.
/// </summary>
/// <remarks>
///     <para>
///         An operation tells the history what it is about to touch rather than the history working it out
///         afterwards. The alternative would be to remember the whole table before every operation and
///         compare it afterwards, which on a table of any size would cost more than the operation itself.
///     </para>
///     <para>
///         Knowing which part of the table is being changed is also what makes an operation that the store
///         performs with a statement, rather than through the change tracker, just as recordable as one it
///         performs through it - which matters, because most of the operations that change a lot of a table
///         at once are statements.
///     </para>
/// </remarks>
public sealed record TableRegion
{
    private TableRegion(TableRegionKind kind)
    {
        Kind = kind;
    }

    /// <summary>
    ///     Which part of the table this is.
    /// </summary>
    public TableRegionKind Kind { get; }

    /// <summary>
    ///     The rows the region covers, when it covers rows.
    /// </summary>
    public IReadOnlyList<int> RowIds { get; private init; } = [];

    /// <summary>
    ///     The columns whose values are wanted, when the region covers columns.
    /// </summary>
    public IReadOnlyList<int> CellColumnIds { get; private init; } = [];

    /// <summary>
    ///     The cells the region covers, when it covers cells.
    /// </summary>
    public IReadOnlyList<(int RowId, int ColumnId)> CellKeys { get; private init; } = [];

    /// <summary>
    ///     The direction of the turn, when the region is a rotation.
    /// </summary>
    public TableRotation Rotation { get; private init; }

    /// <summary>
    ///     The named rows, with the values they hold. Used by an operation that takes rows out.
    /// </summary>
    public static TableRegion Rows(IReadOnlyCollection<int> rowIds)
        => new(TableRegionKind.Rows) { RowIds = [.. rowIds] };

    /// <summary>
    ///     The named cells, with the values they hold.
    /// </summary>
    public static TableRegion Cells(IReadOnlyCollection<(int RowId, int ColumnId)> keys)
        => new(TableRegionKind.Cells) { CellKeys = [.. keys] };

    /// <summary>
    ///     What the columns of the table are, and optionally the values of the ones named.
    /// </summary>
    /// <param name="withCellIds">
    ///     The columns whose values are wanted as well. A column that is being renamed needs none, and a
    ///     column that is being removed needs all of its.
    /// </param>
    public static TableRegion Columns(IReadOnlyCollection<int>? withCellIds = null)
        => new(TableRegionKind.Columns) { CellColumnIds = withCellIds is null ? [] : [.. withCellIds] };

    /// <summary>
    ///     The order the table keeps its rows in.
    /// </summary>
    public static TableRegion RowOrder()
        => new(TableRegionKind.RowOrder);

    /// <summary>
    ///     A quarter turn of the whole table, which needs no values remembered because a turn is undone by
    ///     turning the other way.
    /// </summary>
    public static TableRegion Rotated(TableRotation rotation)
        => new(TableRegionKind.Rotation) { Rotation = rotation };

    /// <summary>
    ///     The whole of the table, which only an operation that replaces it asks for.
    /// </summary>
    public static TableRegion Table()
        => new(TableRegionKind.Table);
}

/// <summary>
///     What a region held when it was read.
/// </summary>
internal sealed record TableRegionState
{
    public List<RowValues>? Rows { get; init; }

    public List<CellValue>? Cells { get; init; }

    public List<ColumnValues>? Columns { get; init; }

    public List<int>? RowOrder { get; init; }

    public TableSnapshot? Table { get; init; }
}

/// <summary>
///     Reads a region of a table, and works out what changed between two readings of it.
/// </summary>
internal static class TableRegionCapture
{
    /// <summary>
    ///     Reads what a region holds.
    /// </summary>
    public static async Task<TableRegionState> CaptureAsync(
        TableStoreDbContext db,
        TableRegion region,
        CancellationToken cancellationToken = default)
    {
        switch (region.Kind)
        {
            case TableRegionKind.Rows:
                return new TableRegionState
                {
                    Rows = await TableSnapshotReader.ReadRowsAsync(db, region.RowIds, cancellationToken)
                        .ConfigureAwait(false),
                };

            case TableRegionKind.Cells:
                return new TableRegionState
                {
                    Cells = await TableSnapshotReader.ReadCellsAsync(db, region.CellKeys, cancellationToken)
                        .ConfigureAwait(false),
                };

            case TableRegionKind.Columns:
            {
                // Every column is read rather than only the ones named, because removing a column closes the
                // gap it leaves in the ordinal positions and so moves the columns that followed it.
                var columnIds = await db.Columns
                    .AsNoTracking()
                    .Select(column => column.Id)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                return new TableRegionState
                {
                    Columns = await TableSnapshotReader
                        .ReadColumnsAsync(db, columnIds, region.CellColumnIds, cancellationToken)
                        .ConfigureAwait(false),
                };
            }

            case TableRegionKind.RowOrder:
                return new TableRegionState
                {
                    RowOrder = await TableSnapshotReader.ReadRowOrderAsync(db, cancellationToken)
                        .ConfigureAwait(false),
                };

            case TableRegionKind.Rotation:
                return new TableRegionState();

            case TableRegionKind.Table:
                return new TableRegionState
                {
                    Table = await TableSnapshotReader.ReadTableAsync(db, cancellationToken).ConfigureAwait(false),
                };

            default:
                throw new ArgumentOutOfRangeException(nameof(region), region.Kind, "Unknown table region.");
        }
    }

    /// <summary>
    ///     Works out the step that takes the table from what the region held to what it holds now.
    /// </summary>
    /// <param name="region">The region that was read.</param>
    /// <param name="before">What the region held before the operation.</param>
    /// <param name="after">What the region holds now.</param>
    /// <returns>The step, or <see langword="null" /> when the region holds what it held before.</returns>
    public static TableEdit? Diff(TableRegion region, TableRegionState before, TableRegionState after)
    {
        switch (region.Kind)
        {
            case TableRegionKind.Rows:
                return DiffRows(before.Rows ?? [], after.Rows ?? []);

            case TableRegionKind.Cells:
                return DiffCells(before.Cells ?? [], after.Cells ?? []);

            case TableRegionKind.Columns:
                return DiffColumns(before.Columns ?? [], after.Columns ?? []);

            case TableRegionKind.RowOrder:
                return DiffRowOrder(before.RowOrder ?? [], after.RowOrder ?? []);

            case TableRegionKind.Rotation:
                return new RotationEdit { Direction = region.Rotation };

            case TableRegionKind.Table:
                return DiffTable(before.Table, after.Table);

            default:
                throw new ArgumentOutOfRangeException(nameof(region), region.Kind, "Unknown table region.");
        }
    }

    private static TableEdit? DiffRows(List<RowValues> before, List<RowValues> after)
    {
        var beforeById = before.ToDictionary(row => row.RowId);
        var afterById = after.ToDictionary(row => row.RowId);

        var removed = before.Where(row => !afterById.ContainsKey(row.RowId)).ToList();
        var added = after.Where(row => !beforeById.ContainsKey(row.RowId)).ToList();

        var changes = new List<CellChange>();

        foreach (var row in before)
        {
            if (afterById.TryGetValue(row.RowId, out var afterRow))
            {
                changes.AddRange(DiffRowCells(row, afterRow));
                continue;
            }

            // A row that is no longer there takes its values out of the table with it, so they are held by
            // the row being put back rather than by a value change of their own.
        }

        return Combine(
            removed.Count > 0 || added.Count > 0
                ? new RowsEdit { Removed = removed, Added = added }
                : null,
            changes.Count > 0 ? new CellValuesEdit { Changes = changes } : null);
    }

    private static TableEdit? DiffCells(List<CellValue> before, List<CellValue> after)
    {
        var beforeByKey = before.ToDictionary(cell => (cell.RowId, cell.ColumnId));
        var afterByKey = after.ToDictionary(cell => (cell.RowId, cell.ColumnId));

        var changes = new List<CellChange>();

        foreach (var cell in before)
        {
            var key = (cell.RowId, cell.ColumnId);

            // A cell that is no longer there reads as one that holds nothing, which is how a value that is
            // missing is stored anyway, so putting the value back is enough to restore it.
            var afterValue = afterByKey.TryGetValue(key, out var current) ? current.Value : null;

            if (!string.Equals(cell.Value, afterValue, StringComparison.Ordinal))
            {
                changes.Add(new CellChange(cell.RowId, cell.ColumnId, cell.Value, afterValue));
            }
        }

        foreach (var cell in after)
        {
            if (!beforeByKey.ContainsKey((cell.RowId, cell.ColumnId)))
            {
                changes.Add(new CellChange(cell.RowId, cell.ColumnId, null, cell.Value));
            }
        }

        return changes.Count > 0 ? new CellValuesEdit { Changes = changes } : null;
    }

    private static TableEdit? DiffColumns(List<ColumnValues> before, List<ColumnValues> after)
    {
        if (before.Count == after.Count)
        {
            var unchanged = true;

            for (var index = 0; index < before.Count && unchanged; index++)
            {
                unchanged = SameColumn(before[index], after[index]);
            }

            if (unchanged)
            {
                return null;
            }
        }

        return new ColumnsEdit { Before = before, After = after };
    }

    /// <summary>
    ///     Whether a column is what it was, comparing the values under it only when both readings took them.
    /// </summary>
    private static bool SameColumn(ColumnValues before, ColumnValues after)
    {
        if (before.ColumnId != after.ColumnId
            || before.Name != after.Name
            || before.DataType != after.DataType
            || before.DefaultValueForCell != after.DefaultValueForCell
            || before.OrdinalPosition != after.OrdinalPosition)
        {
            return false;
        }

        if (before.Cells is null || after.Cells is null || before.Cells.Count != after.Cells.Count)
        {
            return before.Cells is null || after.Cells is null;
        }

        return before.Cells.All(cell =>
            after.Cells.TryGetValue(cell.Key, out var value)
            && string.Equals(cell.Value, value, StringComparison.Ordinal));
    }

    private static TableEdit? DiffRowOrder(List<int> before, List<int> after)
    {
        return before.SequenceEqual(after) ? null : new RowOrderEdit { Before = before, After = after };
    }

    private static TableEdit? DiffTable(TableSnapshot? before, TableSnapshot? after)
    {
        if (before is null || after is null)
        {
            return null;
        }

        return before == after ? null : new TableReplaceEdit { Before = before, After = after };
    }

    private static IEnumerable<CellChange> DiffRowCells(RowValues before, RowValues after)
    {
        foreach (var (columnId, beforeValue) in before.Cells)
        {
            after.Cells.TryGetValue(columnId, out var afterValue);

            if (!string.Equals(beforeValue, afterValue, StringComparison.Ordinal))
            {
                yield return new CellChange(before.RowId, columnId, beforeValue, afterValue);
            }
        }

        foreach (var (columnId, afterValue) in after.Cells)
        {
            if (!before.Cells.ContainsKey(columnId))
            {
                yield return new CellChange(before.RowId, columnId, null, afterValue);
            }
        }
    }

    private static TableEdit? Combine(TableEdit? first, TableEdit? second)
    {
        if (first is not null && second is not null)
        {
            return new CompositeEdit { Edits = [first, second] };
        }

        return first ?? second;
    }
}
