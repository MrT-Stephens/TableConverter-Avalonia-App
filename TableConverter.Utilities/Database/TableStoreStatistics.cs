using Microsoft.EntityFrameworkCore;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Models.TableStore;

namespace TableConverter.Utilities.Database;

/// <summary>
///     What one column of a table holds.
/// </summary>
/// <param name="OrdinalPosition">The column's position in the table, counting from one.</param>
/// <param name="Name">The column's name.</param>
/// <param name="DataType">The type the column was given, which decides how its values read.</param>
/// <param name="FilledCount">The number of rows whose value for this column holds something.</param>
/// <param name="EmptyCount">The number of rows whose value for this column holds nothing.</param>
/// <param name="DistinctCount">The number of different values the column holds.</param>
public sealed record ColumnStatistics(
    int OrdinalPosition,
    string Name,
    ColumnDataType DataType,
    int FilledCount,
    int EmptyCount,
    int DistinctCount);

/// <summary>
///     What a table holds, per column and as a whole.
/// </summary>
/// <param name="RowCount">The number of rows in the table.</param>
/// <param name="ColumnCount">The number of columns in the table.</param>
/// <param name="FilledCellCount">The number of cells that hold something.</param>
/// <param name="EmptyCellCount">The number of cells that hold nothing, counted as the rows that have no value for a column.</param>
/// <param name="Columns">The statistics of every column, in table order.</param>
public sealed record TableStatistics(
    int RowCount,
    int ColumnCount,
    int FilledCellCount,
    int EmptyCellCount,
    IReadOnlyList<ColumnStatistics> Columns);

/// <summary>
///     Reads the statistics of a table store.
/// </summary>
/// <remarks>
///     The counts are worked out by the database rather than by reading the table back: a table can be
///     far larger than the memory it is summarised in, so the summary has to be three queries whatever
///     the table holds.
/// </remarks>
public static class TableStoreStatistics
{
    /// <summary>
    ///     Reads the statistics of the store behind <paramref name="dbContext" />.
    /// </summary>
    /// <param name="dbContext">A context bound to the store to read. The caller owns it.</param>
    /// <param name="cancellationToken">Token used to cancel the read.</param>
    public static async Task<TableStatistics> ReadAsync(
        TableStoreDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        var rowCount = await dbContext.Rows
            .AsNoTracking()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        var columns = await dbContext.Columns
            .AsNoTracking()
            .OrderBy(column => column.OrdinalPosition)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // A cell that holds nothing is filtered out rather than counted, so a column whose cells are all
        // empty simply does not appear in either result and reads as a column of zeroes.
        var filledByColumn = (await dbContext.Cells
                .AsNoTracking()
                .Where(cell => cell.Value != null && cell.Value.Trim() != "")
                .GroupBy(cell => cell.ColumnId)
                .Select(group => new { ColumnId = group.Key, Count = group.Count() })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false))
            .ToDictionary(entry => entry.ColumnId, entry => entry.Count);

        // The distinct values are worked out before they are grouped, so the count is taken over the
        // values a column actually holds rather than over its cells.
        var distinctByColumn = (await dbContext.Cells
                .AsNoTracking()
                .Where(cell => cell.Value != null && cell.Value.Trim() != "")
                .Select(cell => new { cell.ColumnId, cell.Value })
                .Distinct()
                .GroupBy(entry => entry.ColumnId)
                .Select(group => new { ColumnId = group.Key, Count = group.Count() })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false))
            .ToDictionary(entry => entry.ColumnId, entry => entry.Count);

        var statistics = new List<ColumnStatistics>(columns.Count);

        var filledCellCount = 0;

        foreach (var column in columns)
        {
            var filled = filledByColumn.GetValueOrDefault(column.Id);

            filledCellCount += filled;

            statistics.Add(new ColumnStatistics(
                column.OrdinalPosition,
                column.Name,
                column.DataType,
                filled,
                // A row without a cell for this column reads as empty in the grid, so the rows that hold
                // nothing are what is left of the table rather than what the cells add up to.
                (int)Math.Clamp((long)rowCount - filled, 0, int.MaxValue),
                distinctByColumn.GetValueOrDefault(column.Id)));
        }

        var cellCount = (long)rowCount * columns.Count;

        return new TableStatistics(
            rowCount,
            columns.Count,
            filledCellCount,
            (int)Math.Clamp(cellCount - filledCellCount, 0, int.MaxValue),
            statistics);
    }
}

