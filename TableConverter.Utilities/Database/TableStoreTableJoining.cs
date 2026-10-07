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
///     Brings a second table into a table in place: its rows are merged in, and its columns are joined in.
/// </summary>
/// <remarks>
///     <para>
///         Both operations change the table they are run on rather than making a table of their own, because
///         bringing another table in is how a table is made whole rather than how a new one is made. A merge
///         adds the rows of the other table after the rows the table already holds, matching the columns up
///         by name and adding any the table does not yet have. A join adds the columns of the other table to
///         the rows that match them on the key columns.
///     </para>
///     <para>
///         The whole of it is one step in the table's history, so a merge or a join that turned out not to be
///         wanted is taken back the way any other change is.
///     </para>
/// </remarks>
public static class TableStoreTableJoining
{
    /// <summary>
    ///     How many rows are read and written at a time.
    /// </summary>
    public const int DefaultBatchSize = 500;

    /// <summary>
    ///     Separates the key values of a row into the string a match is looked up by. It is the ASCII unit
    ///     separator, which does not occur in text a person types.
    /// </summary>
    private const char KeySeparator = '\u001f';

    /// <summary>
    ///     Merges the rows of another table into this one, adding them after the rows already held.
    /// </summary>
    /// <param name="dbContext">A context bound to the store to change. The caller owns it.</param>
    /// <param name="history">The history the whole operation is recorded in as one step.</param>
    /// <param name="path">The store being changed, which is how the history names it.</param>
    /// <param name="source">The table whose rows are merged in.</param>
    /// <param name="description">How the step is to be described in the history.</param>
    /// <param name="batchSize">How many rows to read and write at a time.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>How many rows were added, and the columns the table gained from the other table.</returns>
    /// <remarks>
    ///     The columns are matched up by name, so a column of the other table that the table already holds by
    ///     that name has its values written under the column that is already there. A column the table does
    ///     not yet hold is added, and a column of the table the other table does not hold reads as nothing
    ///     for the rows that were added.
    /// </remarks>
    public static async Task<TableMergeResult> AppendAsync(
        TableStoreDbContext dbContext,
        ITableHistory history,
        string path,
        ITableRowSource source,
        string description,
        int batchSize = DefaultBatchSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(history);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);

        var columns = await ReadColumnsAsync(dbContext, cancellationToken).ConfigureAwait(false);
        var sourceColumns = await source.GetColumnsAsync(cancellationToken).ConfigureAwait(false);

        // The columns are matched up by name, so a value lands under the column of the table that goes by the
        // same name as the column it was read from rather than under whichever column happens to sit there.
        var indexByName = new Dictionary<string, int>(StringComparer.Ordinal);

        for (var index = 0; index < columns.Count; index++)
        {
            indexByName.TryAdd(columns[index].Name, index);
        }

        var added = new List<ColumnEntity>();
        var sourceToTarget = new int[sourceColumns.Count];
        var nextId = columns.Count == 0 ? 1 : columns.Max(column => column.Id) + 1;
        var nextOrdinal = columns.Count == 0 ? 1 : columns.Max(column => column.OrdinalPosition) + 1;

        for (var index = 0; index < sourceColumns.Count; index++)
        {
            if (indexByName.TryGetValue(sourceColumns[index].Name, out var existing))
            {
                sourceToTarget[index] = existing;
                continue;
            }

            var target = columns.Count + added.Count;

            added.Add(new ColumnEntity
            {
                Id = nextId++,
                Name = sourceColumns[index].Name,
                DataType = sourceColumns[index].EffectiveDataType,
                OrdinalPosition = nextOrdinal++,
            });

            indexByName[sourceColumns[index].Name] = target;
            sourceToTarget[index] = target;
        }

        // The ids of the table's columns once the ones the other table brought have been added, so a value
        // can be written against the column index it belongs to.
        var targetIds = columns.Select(column => column.Id).Concat(added.Select(column => column.Id)).ToList();

        await using var edit = history.BeginEdit(path, TableEditKind.RowsAdded, description);

        // The columns the table gains are remembered by what they are rather than by what they hold: the
        // values that go under them belong to the rows that are added, and are remembered with them.
        await edit.CaptureBeforeAsync(TableRegion.Columns(), cancellationToken).ConfigureAwait(false);

        var rowIds = new List<int>();

        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (added.Count > 0)
            {
                dbContext.Columns.AddRange(added);

                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                foreach (var column in added)
                {
                    dbContext.Entry(column).State = EntityState.Detached;
                }
            }

            // How many rows are waiting to be written. They are written a batch at a time so the whole of
            // the other table never has to be held in memory at once.
            var pending = 0;

            await foreach (var sourceRow in source.ReadRowsAsync(cancellationToken).ConfigureAwait(false))
            {
                var row = new RowEntity();

                for (var index = 0; index < sourceColumns.Count; index++)
                {
                    var value = index < sourceRow.Length ? sourceRow[index] : null;

                    // A value that reads as nothing is left unwritten rather than written as an empty cell,
                    // which is how an unset value is stored anyway.
                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }

                    row.Cells.Add(new CellEntity
                    {
                        ColumnId = targetIds[sourceToTarget[index]],
                        Value = value,
                    });
                }

                dbContext.Rows.Add(row);
                pending++;

                if (pending < batchSize)
                {
                    continue;
                }

                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                // The batch is written, so the ids it was given are read and the rows are let go of rather
                // than kept in memory for the rest of the walk.
                AppendNewRowIds(dbContext, rowIds);
                dbContext.ChangeTracker.Clear();
                pending = 0;
            }

            // The rows added after the last full batch are written whether or not the batch was full.
            if (pending > 0)
            {
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                AppendNewRowIds(dbContext, rowIds);
                dbContext.ChangeTracker.Clear();
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }

        // The rows the table gained are remembered once they are down, so the step holds every value that was
        // written under every column of them, the ones that were already there included.
        if (rowIds.Count > 0)
        {
            await edit.CaptureAfterAsync(TableRegion.Rows(rowIds), cancellationToken).ConfigureAwait(false);
        }

        await edit.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new TableMergeResult(rowIds.Count, added);
    }

    /// <summary>
    ///     Joins the columns of another table onto this one, matched on the key columns.
    /// </summary>
    /// <param name="dbContext">A context bound to the store to change. The caller owns it.</param>
    /// <param name="history">The history the whole operation is recorded in as one step.</param>
    /// <param name="path">The store being changed, which is how the history names it.</param>
    /// <param name="source">The table whose columns are joined on.</param>
    /// <param name="matches">The columns the two tables are matched on, read from this table and from the other.</param>
    /// <param name="description">How the step is to be described in the history.</param>
    /// <param name="batchSize">How many rows to read and write at a time.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    ///     How many of this table's rows found a match, and the columns this table gained from the other.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         Every column of the other table that is not one of the keys is added to this table, and each of
    ///         this table's rows takes the values of the first row of the other table that matches it. A row
    ///         that matches nothing keeps nothing under the new columns, which is how a value that is not
    ///         there reads everywhere else.
    ///     </para>
    ///     <para>
    ///         The other table is read into memory once, keyed by its key columns, so a join costs as much
    ///         memory as the table being joined on holds rather than as much as this table holds.
    ///     </para>
    /// </remarks>
    public static async Task<TableJoinResult> JoinAsync(
        TableStoreDbContext dbContext,
        ITableHistory history,
        string path,
        ITableRowSource source,
        IReadOnlyList<ColumnMatch> matches,
        string description,
        int batchSize = DefaultBatchSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(history);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(matches);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);

        if (matches.Count == 0)
        {
            throw new ArgumentException("At least one pair of columns has to be matched.", nameof(matches));
        }

        var columns = await ReadColumnsAsync(dbContext, cancellationToken).ConfigureAwait(false);
        var sourceColumns = await source.GetColumnsAsync(cancellationToken).ConfigureAwait(false);

        var targetKeyIndices = matches.Select(match => IndexOfColumn(columns, match.DestinationColumn)).ToArray();
        var sourceKeyIndices = matches.Select(match => IndexOfColumn(sourceColumns, match.SourceColumn)).ToArray();
        var targetKeyIds = targetKeyIndices.Select(index => columns[index].Id).ToArray();

        // The key columns are the join itself, so they are not brought across a second time.
        var keySources = sourceKeyIndices.ToHashSet();
        var brought = Enumerable.Range(0, sourceColumns.Count).Where(index => !keySources.Contains(index)).ToList();

        if (brought.Count == 0)
        {
            return new TableJoinResult(0, []);
        }

        // The other table is read once into a lookup, so each of this table's rows can be matched to it
        // without reading it again. The first row of the other table that holds a key is the one that matches.
        var lookup = new Dictionary<string, string?[]>(StringComparer.Ordinal);

        await foreach (var sourceRow in source.ReadRowsAsync(cancellationToken).ConfigureAwait(false))
        {
            var key = BuildKey(sourceKeyIndices.Select(index => ValueAt(sourceRow, index)));

            if (key is null || lookup.ContainsKey(key))
            {
                continue;
            }

            lookup[key] = [.. brought.Select(index => ValueAt(sourceRow, index))];
        }

        var nextId = columns.Count == 0 ? 1 : columns.Max(column => column.Id) + 1;
        var nextOrdinal = columns.Count == 0 ? 1 : columns.Max(column => column.OrdinalPosition) + 1;

        var added = new List<ColumnEntity>(brought.Count);

        foreach (var index in brought)
        {
            added.Add(new ColumnEntity
            {
                Id = nextId++,
                Name = sourceColumns[index].Name,
                DataType = sourceColumns[index].EffectiveDataType,
                OrdinalPosition = nextOrdinal++,
            });
        }

        await using var edit = history.BeginEdit(path, TableEditKind.ColumnsChanged, description);
        await edit.CaptureBeforeAsync(TableRegion.Columns([.. added.Select(column => column.Id)]),
            cancellationToken).ConfigureAwait(false);

        var matched = 0;

        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            dbContext.Columns.AddRange(added);

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            foreach (var column in added)
            {
                dbContext.Entry(column).State = EntityState.Detached;
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

                // Only the values the key is read from are pulled back, so a wide table does not have to be
                // carried through memory to match its rows.
                var keyCells = await dbContext.Cells
                    .AsNoTracking()
                    .Where(cell => rowIds.Contains(cell.RowId) && targetKeyIds.Contains(cell.ColumnId))
                    .Select(cell => new { cell.RowId, cell.ColumnId, cell.Value })
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                var valuesByRow = keyCells
                    .GroupBy(cell => cell.RowId)
                    .ToDictionary(
                        group => group.Key,
                        group => group.ToDictionary(cell => cell.ColumnId, cell => cell.Value));

                var addedCells = new List<CellEntity>();

                foreach (var rowId in rowIds)
                {
                    valuesByRow.TryGetValue(rowId, out var values);

                    var key = BuildKey(targetKeyIds.Select(id =>
                        values is not null && values.TryGetValue(id, out var value) ? value : null));

                    if (key is null || !lookup.TryGetValue(key, out var match))
                    {
                        continue;
                    }

                    matched++;

                    for (var index = 0; index < added.Count; index++)
                    {
                        var value = match[index];

                        if (string.IsNullOrEmpty(value))
                        {
                            continue;
                        }

                        addedCells.Add(new CellEntity
                        {
                            RowId = rowId,
                            ColumnId = added[index].Id,
                            Value = value,
                        });
                    }
                }

                if (addedCells.Count == 0)
                {
                    continue;
                }

                dbContext.Cells.AddRange(addedCells);

                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                dbContext.ChangeTracker.Clear();
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }

        await edit.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new TableJoinResult(matched, added);
    }

    /// <summary>
    ///     Reads the ids the rows that were just saved were given, so the step can remember them.
    /// </summary>
    /// <remarks>
    ///     A row's id is handed out by the store when it is saved rather than chosen before, so the ids the
    ///     new rows are known by are read back off the tracked rows once they are down. Only the batch that
    ///     was just written is tracked, because the change tracker is cleared after every batch, so the ids
    ///     read back are added to whatever earlier batches have already contributed.
    /// </remarks>
    private static void AppendNewRowIds(TableStoreDbContext dbContext, List<int> rowIds)
    {
        var saved = dbContext.ChangeTracker
            .Entries<RowEntity>()
            .Where(entry => entry.State == EntityState.Unchanged)
            .Select(entry => entry.Entity.Id);

        rowIds.AddRange(saved);
    }

    /// <summary>
    ///     Builds the string a row is matched by, or <see langword="null" /> when one of its key values is not
    ///     there, because a row that is missing part of its key cannot be said to match another.
    /// </summary>
    private static string? BuildKey(IEnumerable<string?> values)
    {
        var parts = new List<string>();

        foreach (var value in values)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            parts.Add(value);
        }

        return string.Join(KeySeparator, parts);
    }

    private static string? ValueAt(IReadOnlyList<string?> row, int index)
    {
        return index >= 0 && index < row.Count ? row[index] : null;
    }

    private static int IndexOfColumn(IReadOnlyList<TableColumn> columns, string name)
    {
        for (var index = 0; index < columns.Count; index++)
        {
            if (string.Equals(columns[index].Name, name, StringComparison.Ordinal))
            {
                return index;
            }
        }

        throw new ArgumentException($"The table has no column called '{name}'.", nameof(columns));
    }

    private static int IndexOfColumn(IReadOnlyList<ColumnEntity> columns, string name)
    {
        for (var index = 0; index < columns.Count; index++)
        {
            if (string.Equals(columns[index].Name, name, StringComparison.Ordinal))
            {
                return index;
            }
        }

        throw new ArgumentException($"The table has no column called '{name}'.", nameof(columns));
    }

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
}

/// <summary>
///     One pair of columns two tables are matched on: the column of this table and the column of the other.
/// </summary>
/// <param name="DestinationColumn">The name of the column of the table being changed.</param>
/// <param name="SourceColumn">The name of the column of the table being brought in.</param>
public sealed record ColumnMatch(string DestinationColumn, string SourceColumn);

/// <summary>
///     What merging another table into this one did.
/// </summary>
/// <param name="AppendedRowCount">How many rows were added after the rows the table already held.</param>
/// <param name="AddedColumns">The columns the table gained from the other table.</param>
public sealed record TableMergeResult(int AppendedRowCount, IReadOnlyList<ColumnEntity> AddedColumns);

/// <summary>
///     What joining another table onto this one did.
/// </summary>
/// <param name="MatchedRowCount">How many of the table's rows found a match in the other table.</param>
/// <param name="AddedColumns">The columns the table gained from the other table.</param>
public sealed record TableJoinResult(int MatchedRowCount, IReadOnlyList<ColumnEntity> AddedColumns);



