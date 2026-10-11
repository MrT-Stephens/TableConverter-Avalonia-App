using Microsoft.EntityFrameworkCore;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Models;

namespace TableConverter.Utilities.Database;

/// <summary>
///     A cell whose value does not read as its column's type.
/// </summary>
/// <param name="RowPosition">
///     The place the row holds in the table, counting from one, which is where the grid shows it. This is
///     the row's rank in the order the table keeps its rows in rather than its id, because a row that was
///     deleted leaves a gap in the ids that the rows after it do not move up to fill.
/// </param>
/// <param name="ColumnOrdinal">The place the column holds in the table, counting from one.</param>
/// <param name="ColumnId">The id of the column the cell belongs to.</param>
/// <param name="ColumnName">The name of the column the cell belongs to.</param>
/// <param name="Value">The value as it was stored, which is the text that does not read as the type.</param>
public sealed record InvalidCell(
    int RowPosition,
    int ColumnOrdinal,
    int ColumnId,
    string ColumnName,
    string Value);

/// <summary>
///     What one column holds that does not read as its type.
/// </summary>
/// <param name="ColumnId">The id of the column.</param>
/// <param name="ColumnName">The name of the column.</param>
/// <param name="DataType">The type the column was given.</param>
/// <param name="InvalidCount">How many of the column's values do not read as that type.</param>
public sealed record ColumnValidationSummary(
    int ColumnId,
    string ColumnName,
    ColumnDataType DataType,
    int InvalidCount);

/// <summary>
///     What a table holds that does not read as the type of the column it is under.
/// </summary>
/// <param name="RowCount">The number of rows in the table.</param>
/// <param name="ColumnCount">The number of columns in the table.</param>
/// <param name="InvalidCellCount">How many cells hold a value that does not read as their column's type.</param>
/// <param name="IsTruncated">
///     Whether more cells are faulty than <paramref name="Cells" /> names, which happens when the table
///     holds more faults than the report was asked to describe.
/// </param>
/// <param name="Columns">The columns that hold at least one faulty value, in table order.</param>
/// <param name="Cells">The faulty cells that are described in full, grouped by column and in table order.</param>
public sealed record TableValidationReport(
    int RowCount,
    int ColumnCount,
    int InvalidCellCount,
    bool IsTruncated,
    IReadOnlyList<ColumnValidationSummary> Columns,
    IReadOnlyList<InvalidCell> Cells);

/// <summary>
///     Reads what a table holds that does not read as the type of the column it is under.
/// </summary>
/// <remarks>
///     <para>
///         Nothing is rejected on the strength of a column's type when a value is written, so a value only
///         turns out not to read as its type when it is looked at. This is that look: it reads the cells of
///         the columns that name a type, tests each value the way the grid and the exporters test it, and
///         reports what it found.
///     </para>
///     <para>
///         A column of text, and a column whose type this build does not know, takes anything and is never
///         read, so a table of text costs nothing to check. The cells of the columns that do name a type
///         are streamed rather than loaded, so the read costs one value at a time rather than the whole
///         table at once.
///     </para>
/// </remarks>
public static class TableStoreValidation
{
    /// <summary>
    ///     How many faulty cells are described in full before the rest are only counted.
    /// </summary>
    /// <remarks>
    ///     A table can hold a fault in every one of its cells, so the report describes a bounded sample
    ///     rather than every fault: the counts always describe the whole table, and what is listed is enough
    ///     to work from without holding a second copy of a large table in memory.
    /// </remarks>
    public const int DefaultMaximumReportedCells = 2000;

    /// <summary>
    ///     Reads the validation report of the store behind <paramref name="dbContext" />.
    /// </summary>
    /// <param name="dbContext">A context bound to the store to read. The caller owns it.</param>
    /// <param name="maximumReportedCells">
    ///     How many faulty cells to describe in full. The counts still describe the whole table.
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the read.</param>
    public static async Task<TableValidationReport> ReadAsync(
        TableStoreDbContext dbContext,
        int maximumReportedCells = DefaultMaximumReportedCells,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumReportedCells);

        var columns = await dbContext.Columns
            .AsNoTracking()
            .OrderBy(column => column.OrdinalPosition)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var rowCount = await dbContext.Rows
            .AsNoTracking()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        // Text takes anything, and a type this build does not know is left as the text it already is, so
        // neither can hold a value that does not read. Reading them would cost a scan for no answer.
        var validatingIds = columns
            .Where(column => column.DataType is not ColumnDataType.Text)
            .Select(column => column.Id)
            .ToList();

        if (columns.Count == 0 || validatingIds.Count == 0 || rowCount == 0)
        {
            return new TableValidationReport(rowCount, columns.Count, 0, false, [], []);
        }

        var columnsById = columns.ToDictionary(column => column.Id);

        // The sample is spread over the columns rather than left to fall on whichever columns happen to be
        // read first, so one badly typed column does not hide every other column's faults.
        var perColumnCap = Math.Max(1, maximumReportedCells / validatingIds.Count);

        var invalidByColumn = new Dictionary<int, int>();
        var reportedByColumn = new Dictionary<int, int>();
        var reported = new List<(int RowId, int ColumnId, string Value)>();

        var cells = dbContext.Cells
            .AsNoTracking()
            .Where(cell => validatingIds.Contains(cell.ColumnId)
                           && cell.Value != null
                           && cell.Value.Trim() != "")
            .OrderBy(cell => cell.ColumnId)
            .ThenBy(cell => cell.RowId)
            .Select(cell => new { cell.ColumnId, cell.RowId, cell.Value });

        var invalidCellCount = 0;

        // The cells are read a row at a time rather than loaded, so checking a large table costs one value
        // at a time rather than the whole of it.
        await foreach (var cell in cells.AsAsyncEnumerable().WithCancellation(cancellationToken)
                           .ConfigureAwait(false))
        {
            var column = columnsById[cell.ColumnId];

            if (column.DataType.IsValidValue(cell.Value))
            {
                continue;
            }

            invalidCellCount++;
            invalidByColumn[cell.ColumnId] = invalidByColumn.GetValueOrDefault(cell.ColumnId) + 1;

            var alreadyReported = reportedByColumn.GetValueOrDefault(cell.ColumnId);

            if (alreadyReported >= perColumnCap)
            {
                continue;
            }

            reported.Add((cell.RowId, cell.ColumnId, cell.Value!));
            reportedByColumn[cell.ColumnId] = alreadyReported + 1;
        }

        if (invalidCellCount == 0)
        {
            return new TableValidationReport(rowCount, columns.Count, 0, false, [], []);
        }

        // Only the rows the report names are placed, so the rank is read for those rows rather than for the
        // whole table.
        var positions = await ReadRowPositionsAsync(
            dbContext,
            reported.Select(cell => cell.RowId).Distinct().ToList(),
            cancellationToken).ConfigureAwait(false);

        var summaries = columns
            .Where(column => invalidByColumn.GetValueOrDefault(column.Id) > 0)
            .Select(column => new ColumnValidationSummary(
                column.Id,
                column.Name,
                column.DataType,
                invalidByColumn[column.Id]))
            .ToList();

        var invalidCells = reported
            .Select(cell => new InvalidCell(
                positions.GetValueOrDefault(cell.RowId),
                columnsById[cell.ColumnId].OrdinalPosition,
                cell.ColumnId,
                columnsById[cell.ColumnId].Name,
                cell.Value))
            .ToList();

        return new TableValidationReport(
            rowCount,
            columns.Count,
            invalidCellCount,
            invalidCellCount > invalidCells.Count,
            summaries,
            invalidCells);
    }

    /// <summary>
    ///     Reads the place each of the named rows holds in the table, counting from one.
    /// </summary>
    /// <param name="dbContext">A context bound to the store to read. The caller owns it.</param>
    /// <param name="rowIds">The rows whose places are wanted.</param>
    /// <param name="cancellationToken">Token used to cancel the read.</param>
    /// <remarks>
    ///     The rows are read in the order the table keeps them in and counted until every wanted row has been
    ///     found, so the read stops as soon as it can rather than walking the whole of a large table to place
    ///     a value held near its top.
    /// </remarks>
    private static async Task<Dictionary<int, int>> ReadRowPositionsAsync(
        TableStoreDbContext dbContext,
        IReadOnlyList<int> rowIds,
        CancellationToken cancellationToken)
    {
        var positions = new Dictionary<int, int>(rowIds.Count);

        if (rowIds.Count == 0)
        {
            return positions;
        }

        var wanted = rowIds.ToHashSet();
        var position = 0;

        var ordered = dbContext.Rows
            .AsNoTracking()
            .OrderBy(row => row.Id)
            .Select(row => row.Id);

        await foreach (var rowId in ordered.AsAsyncEnumerable().WithCancellation(cancellationToken)
                           .ConfigureAwait(false))
        {
            position++;

            if (!wanted.Remove(rowId))
            {
                continue;
            }

            positions[rowId] = position;

            if (wanted.Count == 0)
            {
                break;
            }
        }

        return positions;
    }
}

