using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.History;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Utilities.Models;

namespace TableConverter.Utilities.Database;

/// <summary>
///     One column a derived-column operation adds, and how each of its values is worked out.
/// </summary>
/// <param name="Name">What the column is to be called.</param>
/// <param name="DataType">The type its values are to be read as.</param>
/// <param name="Compute">
///     Works out one value from a row's source values, which are handed over in the order the source columns
///     were named. Reading nothing means the cell is left unwritten, which is how an unset value reads.
/// </param>
public sealed record ComputedColumnDefinition(
    string Name,
    ColumnDataType DataType,
    Func<IReadOnlyList<string?>, string?> Compute);

/// <summary>
///     Adds columns to a table whose values are worked out from the values a row already holds.
/// </summary>
/// <remarks>
///     <para>
///         A derived column is added the way any other column is, and its values are then written a row at a
///         time. The whole of it is one step in the table's history, so a column that turned out not to be
///         wanted is taken back the same way any other change is.
///     </para>
///     <para>
///         The rows are read in batches rather than all at once, so a table of any size costs one batch of
///         memory rather than the whole of it, and only the values of the source columns are read rather
///         than the whole of every row.
///     </para>
/// </remarks>
public static class TableStoreComputedColumns
{
    /// <summary>
    ///     How many rows are read and written at a time.
    /// </summary>
    public const int DefaultBatchSize = 500;

    /// <summary>
    ///     Adds the columns the definitions describe to the table, filling each with the value worked out
    ///     for its row.
    /// </summary>
    /// <param name="dbContext">A context bound to the store to change. The caller owns it.</param>
    /// <param name="history">The history the whole operation is recorded in as one step.</param>
    /// <param name="path">The store being changed, which is how the history names it.</param>
    /// <param name="sourceColumnIds">
    ///     The columns whose values are handed to each definition, in this order. A row that holds nothing
    ///     under one of them hands over nothing for it.
    /// </param>
    /// <param name="definitions">The columns to add, in the order they are to sit in the table.</param>
    /// <param name="description">How the step is to be described in the history.</param>
    /// <param name="batchSize">How many rows to read and write at a time.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The columns that were added, with the ids and places the store gave them.</returns>
    public static async Task<IReadOnlyList<ColumnEntity>> AddAsync(
        TableStoreDbContext dbContext,
        ITableHistory history,
        string path,
        IReadOnlyList<int> sourceColumnIds,
        IReadOnlyList<ComputedColumnDefinition> definitions,
        string description,
        int batchSize = DefaultBatchSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(history);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(sourceColumnIds);
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);

        if (definitions.Count == 0)
        {
            return [];
        }

        var columns = await BuildColumnsAsync(dbContext, definitions, cancellationToken).ConfigureAwait(false);
        var newColumnIds = columns.Select(column => column.Id).ToList();

        // Every column is remembered rather than only the ones being added, because the values of a row are
        // read by the places columns sit in - and the values under the new columns are remembered with them,
        // so undoing the step takes the worked-out values out along with the columns they were written under.
        await using var edit = history.BeginEdit(path, TableEditKind.ColumnsChanged, description);
        await edit.CaptureBeforeAsync(TableRegion.Columns(newColumnIds), cancellationToken).ConfigureAwait(false);

        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            dbContext.Columns.AddRange(columns);

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            // The columns are put down before the values are written, so nothing the value work does has to
            // carry them along with it.
            foreach (var column in columns)
            {
                dbContext.Entry(column).State = EntityState.Detached;
            }

            await FillAsync(dbContext, sourceColumnIds, columns, definitions, batchSize, cancellationToken)
                .ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }

        await edit.CommitAsync(cancellationToken).ConfigureAwait(false);

        return columns;
    }

    /// <summary>
    ///     Works out the columns to add, giving each the id and place it will hold.
    /// </summary>
    /// <remarks>
    ///     The ids are worked out before anything is written, because a step that remembers the values under
    ///     a new column has to name the column it remembers them by, and nothing would know the id of a
    ///     column the store has yet to hand one out.
    /// </remarks>
    private static async Task<List<ColumnEntity>> BuildColumnsAsync(
        TableStoreDbContext dbContext,
        IReadOnlyList<ComputedColumnDefinition> definitions,
        CancellationToken cancellationToken)
    {
        var highestId = await dbContext.Columns
            .AsNoTracking()
            .Select(column => (int?)column.Id)
            .MaxAsync(cancellationToken).ConfigureAwait(false) ?? 0;

        var highestOrdinal = await dbContext.Columns
            .AsNoTracking()
            .Select(column => (int?)column.OrdinalPosition)
            .MaxAsync(cancellationToken).ConfigureAwait(false) ?? 0;

        var columns = new List<ColumnEntity>(definitions.Count);

        for (var index = 0; index < definitions.Count; index++)
        {
            columns.Add(new ColumnEntity
            {
                Id = highestId + index + 1,
                Name = definitions[index].Name,
                DataType = definitions[index].DataType,
                OrdinalPosition = highestOrdinal + index + 1,
            });
        }

        return columns;
    }

    /// <summary>
    ///     Walks the rows a batch at a time, working out and writing each new column's value for every row.
    /// </summary>
    private static async Task FillAsync(
        TableStoreDbContext dbContext,
        IReadOnlyList<int> sourceColumnIds,
        IReadOnlyList<ColumnEntity> columns,
        IReadOnlyList<ComputedColumnDefinition> definitions,
        int batchSize,
        CancellationToken cancellationToken)
    {
        if (sourceColumnIds.Count == 0)
        {
            return;
        }

        var lastRowId = 0;

        while (true)
        {
            var rowIds = await dbContext.Rows
                .AsNoTracking()
                .Where(row => row.Id > lastRowId)
                .OrderBy(row => row.Id)
                .Take(batchSize)
                .Select(row => row.Id)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            if (rowIds.Count == 0)
            {
                break;
            }

            lastRowId = rowIds[^1];

            // Only the values the operation reads are pulled back, so a wide table does not have to be
            // carried through memory to work out one column of it.
            var sourceCells = await dbContext.Cells
                .AsNoTracking()
                .Where(cell => rowIds.Contains(cell.RowId) && sourceColumnIds.Contains(cell.ColumnId))
                .Select(cell => new { cell.RowId, cell.ColumnId, cell.Value })
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            var valuesByRow = sourceCells
                .GroupBy(cell => cell.RowId)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToDictionary(cell => cell.ColumnId, cell => cell.Value));

            var added = new List<CellEntity>();

            foreach (var rowId in rowIds)
            {
                valuesByRow.TryGetValue(rowId, out var values);

                var sourceValues = new string?[sourceColumnIds.Count];

                for (var index = 0; index < sourceColumnIds.Count; index++)
                {
                    sourceValues[index] =
                        values is not null && values.TryGetValue(sourceColumnIds[index], out var value)
                            ? value
                            : null;
                }

                for (var index = 0; index < columns.Count; index++)
                {
                    var value = definitions[index].Compute(sourceValues);

                    // A value that reads as nothing is left unwritten rather than written as an empty cell,
                    // which is how an unset value is stored anyway.
                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }

                    added.Add(new CellEntity
                    {
                        RowId = rowId,
                        ColumnId = columns[index].Id,
                        Value = value,
                    });
                }
            }

            if (added.Count == 0)
            {
                continue;
            }

            dbContext.Cells.AddRange(added);

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            // The batch is written, so it is let go of rather than kept in memory for the rest of the walk.
            dbContext.ChangeTracker.Clear();
        }
    }
}

