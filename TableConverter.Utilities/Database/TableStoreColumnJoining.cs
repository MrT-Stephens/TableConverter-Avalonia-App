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
///     Changes the columns of a table in place: several columns are joined into one, and one column is
///     split into several.
/// </summary>
/// <remarks>
///     <para>
///         Both operations rewrite the table the columns belong to rather than producing a second table to
///         compare it with, because joining or parting a column is how a table is tidied up rather than how a
///         new one is made. A join takes the values of several columns and writes the one value they make
///         under a new column that sits where the first of them sat; a split reads one column and writes the
///         parts of each value under new columns that sit where it sat. The columns that were read are taken
///         out when the caller asks, which is what makes the result the tidied table rather than a wider one.
///     </para>
///     <para>
///         The whole of it is one step in the table's history, so a join or a split that turned out not to be
///         wanted is taken back the way any other change is. The step remembers the values under the columns
///         that go as well as those under the columns that arrive, which is what lets it put the table back
///         exactly as it was.
///     </para>
///     <para>
///         Rows are read and written a batch at a time rather than all at once, so a table of any size costs
///         one batch of memory rather than the whole of it.
///     </para>
/// </remarks>
public static class TableStoreColumnJoining
{
    /// <summary>
    ///     How many rows are read and written at a time.
    /// </summary>
    public const int DefaultBatchSize = 500;

    /// <summary>
    ///     Joins the values of several columns of a row into one, writing the result under a new column that
    ///     takes the place of the first of the columns that were read.
    /// </summary>
    /// <param name="dbContext">A context bound to the store to change. The caller owns it.</param>
    /// <param name="history">The history the whole operation is recorded in as one step.</param>
    /// <param name="path">The store being changed, which is how the history names it.</param>
    /// <param name="sourceColumnIds">
    ///     The columns to join, in the order their values are to be joined. The values are read in this order
    ///     rather than in the order the columns sit in, so a join can put a surname after a first name.
    /// </param>
    /// <param name="resultName">What the joined column is to be called.</param>
    /// <param name="separator">What to put between the values. Empty when they are to be run together.</param>
    /// <param name="skipBlank">Whether a value that is unset is left out rather than joined as nothing.</param>
    /// <param name="removeSourceColumns">
    ///     Whether the columns that were read are taken out once their values have been joined, which is
    ///     what leaves the table tidied rather than wider.
    /// </param>
    /// <param name="description">How the step is to be described in the history.</param>
    /// <param name="batchSize">How many rows to read and write at a time.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The column that was added, with the id and place the store gave it.</returns>
    public static async Task<IReadOnlyList<ColumnEntity>> JoinColumnsAsync(
        TableStoreDbContext dbContext,
        ITableHistory history,
        string path,
        IReadOnlyList<int> sourceColumnIds,
        string resultName,
        string? separator,
        bool skipBlank,
        bool removeSourceColumns,
        string description,
        int batchSize = DefaultBatchSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(history);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(sourceColumnIds);
        ArgumentException.ThrowIfNullOrWhiteSpace(resultName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);

        if (sourceColumnIds.Count == 0)
        {
            throw new ArgumentException("At least one column has to be joined.", nameof(sourceColumnIds));
        }

        var columns = await ReadColumnsAsync(dbContext, cancellationToken).ConfigureAwait(false);

        // Every column that is named has to be there, so a join that names a column the table does not hold
        // is refused rather than silently joining the columns that did resolve.
        var order = sourceColumnIds.ToList();

        foreach (var id in order)
        {
            _ = FindColumn(columns, id);
        }

        // The result sits where the leftmost column that was read sat, so a join reads as a change to that
        // column rather than as a new one appearing at the far end of the table.
        var insertOrdinal = order.Min(id => FindColumn(columns, id).OrdinalPosition);
        var separatorText = separator ?? string.Empty;

        return await ApplyAsync(
            dbContext,
            history,
            path,
            columns,
            removeSourceColumns ? order : [],
            [new NewColumn(NextColumnId(columns), resultName, ColumnDataType.Text)],
            order,
            insertOrdinal,
            values =>
            {
                var parts = new string?[order.Count];

                for (var index = 0; index < order.Count; index++)
                {
                    values.TryGetValue(order[index], out var value);
                    parts[index] = value;
                }

                return [ComputedValueOperations.Concatenate(parts, separatorText, skipBlank)];
            },
            batchSize,
            description,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Splits the values of one column into several, writing the parts under new columns that take the
    ///     place of the column that was read.
    /// </summary>
    /// <param name="dbContext">A context bound to the store to change. The caller owns it.</param>
    /// <param name="history">The history the whole operation is recorded in as one step.</param>
    /// <param name="path">The store being changed, which is how the history names it.</param>
    /// <param name="sourceColumnId">The column whose values are to be split.</param>
    /// <param name="separator">What each value is split on.</param>
    /// <param name="partNames">
    ///     What each part is to be called, in the order the parts are to sit. How many there are is how many
    ///     columns are made, so a value with more parts than that loses the ones beyond the last.
    /// </param>
    /// <param name="removeSourceColumn">
    ///     Whether the column that was read is taken out once its values have been split, which is what
    ///     leaves the table tidied rather than wider.
    /// </param>
    /// <param name="description">How the step is to be described in the history.</param>
    /// <param name="batchSize">How many rows to read and write at a time.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The columns that were added, in the order they sit in the table.</returns>
    public static async Task<IReadOnlyList<ColumnEntity>> SplitColumnAsync(
        TableStoreDbContext dbContext,
        ITableHistory history,
        string path,
        int sourceColumnId,
        string separator,
        IReadOnlyList<string> partNames,
        bool removeSourceColumn,
        string description,
        int batchSize = DefaultBatchSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(history);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(partNames);
        ArgumentException.ThrowIfNullOrEmpty(separator);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);

        if (partNames.Count == 0)
        {
            throw new ArgumentException("At least one part has to be taken out of the column.", nameof(partNames));
        }

        var columns = await ReadColumnsAsync(dbContext, cancellationToken).ConfigureAwait(false);
        var source = FindColumn(columns, sourceColumnId);
        var firstId = NextColumnId(columns);

        var newColumns = partNames
            .Select((name, index) => new NewColumn(firstId + index, name, ColumnDataType.Text))
            .ToList();

        return await ApplyAsync(
            dbContext,
            history,
            path,
            columns,
            removeSourceColumn ? [sourceColumnId] : [],
            newColumns,
            [sourceColumnId],
            source.OrdinalPosition,
            values =>
            {
                values.TryGetValue(sourceColumnId, out var value);

                return [.. partNames.Select((_, index) =>
                    ComputedValueOperations.SplitPart(value, separator, index))];
            },
            batchSize,
            description,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Rewrites the table's columns to add the new ones and take the named ones out, then fills the new
    ///     columns a row at a time.
    /// </summary>
    /// <remarks>
    ///     The columns are put down before the values are written, so nothing the value work does has to
    ///     carry them along with it, and the whole of it runs in one transaction so a failure part way leaves
    ///     the table exactly as it was.
    /// </remarks>
    private static async Task<IReadOnlyList<ColumnEntity>> ApplyAsync(
        TableStoreDbContext dbContext,
        ITableHistory history,
        string path,
        IReadOnlyList<ColumnEntity> existing,
        IReadOnlyCollection<int> removedIds,
        IReadOnlyList<NewColumn> newColumns,
        IReadOnlyList<int> sourceColumnIds,
        int insertOrdinal,
        Func<IReadOnlyDictionary<int, string?>, IReadOnlyList<string?>> compute,
        int batchSize,
        string description,
        CancellationToken cancellationToken)
    {
        var layout = BuildLayout(existing, removedIds, newColumns, insertOrdinal);

        // The values under the columns that go and those under the columns that arrive are remembered, so
        // the step can put the table back the way it was whichever way it is walked.
        var capturedIds = new List<int>(sourceColumnIds.Count + newColumns.Count);
        capturedIds.AddRange(sourceColumnIds);
        capturedIds.AddRange(newColumns.Select(column => column.Id));

        await using var edit = history.BeginEdit(path, TableEditKind.ColumnsChanged, description);
        await edit.CaptureBeforeAsync(TableRegion.Columns(capturedIds), cancellationToken).ConfigureAwait(false);

        var added = new List<ColumnEntity>(newColumns.Count);

        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // The new columns are put down first, so the values worked out for them have columns to be
            // written against. The places they will hold are settled once every value has been read; until
            // then they sit after the columns the table already holds, which keeps their ids and places clear
            // of the ones that are about to be taken out.
            var highestOrdinal = await dbContext.Columns
                .AsNoTracking()
                .Select(column => (int?)column.OrdinalPosition)
                .MaxAsync(cancellationToken)
                .ConfigureAwait(false) ?? 0;

            foreach (var newColumn in newColumns)
            {
                var entity = new ColumnEntity
                {
                    Id = newColumn.Id,
                    Name = newColumn.Name,
                    DataType = newColumn.DataType,
                    OrdinalPosition = ++highestOrdinal,
                };

                added.Add(entity);
                dbContext.Columns.Add(entity);
            }

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            // The columns are let go of once they are down, so the value work does not have to carry them.
            foreach (var entity in added)
            {
                dbContext.Entry(entity).State = EntityState.Detached;
            }

            // The values are read while the columns that hold them are still there, because taking a column
            // out takes its values with it and there would be nothing left to join or split afterwards.
            await FillAsync(dbContext, sourceColumnIds, added, compute, batchSize, cancellationToken)
                .ConfigureAwait(false);

            // The columns that were read are taken out and every column is put in the place the layout gives
            // it, which closes the gap a taken-out column leaves and opens the one an added column needs.
            var current = await dbContext.Columns
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var currentById = current.ToDictionary(column => column.Id);

            var removed = current.Where(column => removedIds.Contains(column.Id)).ToList();

            if (removed.Count > 0)
            {
                dbContext.Columns.RemoveRange(removed);
            }

            foreach (var slot in layout)
            {
                if (currentById.TryGetValue(slot.Id, out var entity) && entity.OrdinalPosition != slot.Ordinal)
                {
                    entity.OrdinalPosition = slot.Ordinal;
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }

        // The places the new columns were given before they were put down are replaced with the places the
        // layout settled on, so what is handed back describes the table as it now is.
        var settledOrdinals = layout
            .Where(slot => slot.IsNew)
            .ToDictionary(slot => slot.Id, slot => slot.Ordinal);

        foreach (var entity in added)
        {
            if (settledOrdinals.TryGetValue(entity.Id, out var ordinal))
            {
                entity.OrdinalPosition = ordinal;
            }
        }

        await edit.CommitAsync(cancellationToken).ConfigureAwait(false);

        return added;
    }

    /// <summary>
    ///     Walks the rows a batch at a time, working out and writing the value each new column takes for every
    ///     row.
    /// </summary>
    private static async Task FillAsync(
        TableStoreDbContext dbContext,
        IReadOnlyList<int> sourceColumnIds,
        IReadOnlyList<ColumnEntity> newColumns,
        Func<IReadOnlyDictionary<int, string?>, IReadOnlyList<string?>> compute,
        int batchSize,
        CancellationToken cancellationToken)
    {
        if (newColumns.Count == 0 || sourceColumnIds.Count == 0)
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
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

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
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var valuesByRow = sourceCells
                .GroupBy(cell => cell.RowId)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyDictionary<int, string?>)group.ToDictionary(
                        cell => cell.ColumnId,
                        cell => cell.Value));

            var added = new List<CellEntity>();

            foreach (var rowId in rowIds)
            {
                var values = valuesByRow.TryGetValue(rowId, out var row)
                    ? row
                    : EmptyRow;

                var computed = compute(values);

                for (var index = 0; index < newColumns.Count && index < computed.Count; index++)
                {
                    var value = computed[index];

                    // A value that reads as nothing is left unwritten rather than written as an empty cell,
                    // which is how an unset value is stored anyway.
                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }

                    added.Add(new CellEntity
                    {
                        RowId = rowId,
                        ColumnId = newColumns[index].Id,
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

    /// <summary>
    ///     Works out where every column of the rewritten table sits, giving each one its place counting from
    ///     one.
    /// </summary>
    /// <remarks>
    ///     The added columns are gathered where the leftmost column that was read sat, and the columns that
    ///     were taken out simply do not appear. Everything is then renumbered from one, because taking a
    ///     column out closes the gap it leaves rather than leaving a hole in the places the columns hold.
    /// </remarks>
    private static List<ColumnSlot> BuildLayout(
        IReadOnlyList<ColumnEntity> existing,
        IReadOnlyCollection<int> removedIds,
        IReadOnlyList<NewColumn> newColumns,
        int insertOrdinal)
    {
        var slots = new List<ColumnSlot>(existing.Count + newColumns.Count);
        var inserted = false;

        foreach (var column in existing.OrderBy(column => column.OrdinalPosition).ThenBy(column => column.Id))
        {
            if (!inserted && column.OrdinalPosition >= insertOrdinal)
            {
                slots.AddRange(newColumns.Select(added => new ColumnSlot(added.Id, true, 0)));
                inserted = true;
            }

            if (removedIds.Contains(column.Id))
            {
                continue;
            }

            slots.Add(new ColumnSlot(column.Id, false, 0));
        }

        if (!inserted)
        {
            slots.AddRange(newColumns.Select(added => new ColumnSlot(added.Id, true, 0)));
        }

        var layout = new List<ColumnSlot>(slots.Count);

        for (var index = 0; index < slots.Count; index++)
        {
            layout.Add(slots[index] with { Ordinal = index + 1 });
        }

        return layout;
    }

    private static readonly IReadOnlyDictionary<int, string?> EmptyRow =
        new Dictionary<int, string?>();

    private static async Task<List<ColumnEntity>> ReadColumnsAsync(
        TableStoreDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return await dbContext.Columns
            .AsNoTracking()
            .OrderBy(column => column.OrdinalPosition)
            .ThenBy(column => column.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private static ColumnEntity FindColumn(IReadOnlyList<ColumnEntity> columns, int id)
    {
        foreach (var column in columns)
        {
            if (column.Id == id)
            {
                return column;
            }
        }

        throw new ArgumentException($"The table has no column with id {id}.", nameof(columns));
    }

    private static int NextColumnId(IReadOnlyList<ColumnEntity> columns)
    {
        return columns.Count == 0 ? 1 : columns.Max(column => column.Id) + 1;
    }

    /// <summary>
    ///     One column the operation adds, and what it is to be called.
    /// </summary>
    private sealed record NewColumn(int Id, string Name, ColumnDataType DataType);

    /// <summary>
    ///     Where one column sits in the table the operation leaves behind.
    /// </summary>
    private sealed record ColumnSlot(int Id, bool IsNew, int Ordinal);
}

