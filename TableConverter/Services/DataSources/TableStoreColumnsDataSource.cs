using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TableConverter.Services.DataSources.Base;
using TableConverter.Utilities.Database.History;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Models;

namespace TableConverter.Services.DataSources;

/// <summary>
///     Why an edit to a column was refused, so whoever is showing the columns can put them back the way
///     the store holds them and say what was wrong.
/// </summary>
/// <param name="Reason">What was wrong with the edit, in a form short enough to show.</param>
public sealed record ColumnEditRejectedEventArgs(string Reason);

public class TableStoreColumnsDataSource(
    ITableStoreDbContextFactory databaseContextFactory,
    ITableHistory history)
    : DataSourceFromPath<ColumnEntity>(databaseContextFactory, 250, 5)
{
    /// <summary>
    ///     Raised when an edit is refused rather than written, because the store would otherwise be left
    ///     holding a column it cannot describe.
    /// </summary>
    public event EventHandler<ColumnEditRejectedEventArgs>? EditRejected;

    protected override async Task<bool> ContainsAsync(ColumnEntity item)
    {
        if (string.IsNullOrEmpty(Path))
        {
            return false;
        }
        
        if (item is null || item.Id < 0)
        {
            return false;
        }

        await using var db = await CreateDbAsync().ConfigureAwait(false);
        
        return await db.Columns
            .AsNoTracking()
            .AnyAsync(col => col.Id == item.Id)
            .ConfigureAwait(false);
    }

    protected override async Task<int> GetCountAsync(Func<IQueryable<ColumnEntity>, IQueryable<ColumnEntity>> filterQuery)
    {
        if (string.IsNullOrEmpty(Path))
        {
            return 0;
        }
        
        await using var db = await CreateDbAsync().ConfigureAwait(false);
        
        var query = db.Columns.AsNoTracking();

        query = filterQuery(query);

        return await query.CountAsync().ConfigureAwait(false);
    }

    protected override async Task<IEnumerable<ColumnEntity>> GetItemsAtAsync(
        int offset, 
        int count, 
        Func<IQueryable<ColumnEntity>, IQueryable<ColumnEntity>> filterSortQuery)
    {
        if (string.IsNullOrEmpty(Path))
        {
            return [];
        }
        
        await using var db = await CreateDbAsync().ConfigureAwait(false);

        IQueryable<ColumnEntity> query = db.Columns.AsNoTracking();

        query = filterSortQuery(query);

        return await query
            .Skip(offset)
            .Take(count)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public override async Task<ColumnEntity?> GetItemAsync(Expression<Func<ColumnEntity, bool>> predicate)
    {
        if (string.IsNullOrEmpty(Path))
        {
            return null;
        }
        
        await using var db = await CreateDbAsync().ConfigureAwait(false);
        
        return await db.Columns
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate)
            .ConfigureAwait(false);
    }

    protected override ColumnEntity GetPlaceHolder(int index, int page, int offset)
    {
        return new ColumnEntity
        {
            Id = index + 1,
            Name = DataSourcePlaceholder.Text,
            DataType = ColumnDataType.Text,
            DefaultValueForCell = string.Empty
        };
    }

    protected override bool ModelsEqual(ColumnEntity a, ColumnEntity b) => a.Id == b.Id;

    protected override async Task<bool> DoCreateAsync(ColumnEntity item)
    {
        if (string.IsNullOrEmpty(Path))
        {
            return false;
        }

        // A column added from the columns editor changes the shape of the table, so the whole set of
        // columns is remembered rather than only the one added: removing a column renumbers the ones that
        // follow it, and a step has to describe the table it left behind. The set is remembered before the
        // column is there, because a step recorded from a set that already holds the new column would
        // describe an empty table as what came before it and take the whole table back with it.
        await using var edit = history.BeginEdit(Path, TableEditKind.ColumnsChanged, "Added a column");
        await edit.CaptureBeforeAsync(TableRegion.Columns()).ConfigureAwait(false);

        await using var db = await CreateDbAsync().ConfigureAwait(false);
        await using var transaction = await db.Database.BeginTransactionAsync().ConfigureAwait(false);

        try
        {
            var max = await db.Columns
                .AsNoTracking()
                .Select(c => (int?)c.OrdinalPosition)
                .MaxAsync()
                .ConfigureAwait(false);

            // A column added before it has been named still has to read as a column of the table, so it is
            // given a name the table does not already carry rather than left nameless.
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                var taken = await db.Columns
                    .AsNoTracking()
                    .Select(c => c.Name)
                    .ToListAsync()
                    .ConfigureAwait(false);

                item.Name = DefaultName(taken);
            }

            item.OrdinalPosition = (max ?? 0) + 1;

            await db.Columns.AddAsync(item).ConfigureAwait(false);

            await db.SaveChangesAsync().ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync().ConfigureAwait(false);
            throw;
        }

        await edit.CommitAsync().ConfigureAwait(false);

        return true;
    }

    protected override async Task<bool> DoUpdateAsync(ColumnEntity viewModel)
    {
        if (string.IsNullOrEmpty(Path))
        {
            return false;
        }

        // Renaming a column, or changing what it holds, needs no cell values remembered: only the columns
        // themselves are described.
        await using var edit = history.BeginEdit(Path, TableEditKind.ColumnsChanged, "Edited a column");
        await edit.CaptureBeforeAsync(TableRegion.Columns()).ConfigureAwait(false);

        await using var db = await CreateDbAsync().ConfigureAwait(false);
        await using var transaction = await db.Database.BeginTransactionAsync().ConfigureAwait(false);

        try
        {
            var entity = await db.Columns
                .FirstOrDefaultAsync(r => r.Id == viewModel.Id)
                .ConfigureAwait(false);

            if (entity is null)
            {
                return false;
            }

            // The name is trimmed rather than refused for the space around it, so a name that only differs
            // by whitespace is still the name that was typed.
            var name = viewModel.Name?.Trim() ?? string.Empty;

            if (name.Length == 0)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                Reject("A column needs a name.");
                return false;
            }

            // Two columns sharing a name would make every row that reads it ambiguous, so a name already
            // taken by another column is refused rather than written.
            var taken = await db.Columns
                .AsNoTracking()
                .AnyAsync(column => column.Id != viewModel.Id && column.Name == name)
                .ConfigureAwait(false);

            if (taken)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                Reject($"Another column is already called '{name}'.");
                return false;
            }

            entity.Name = name;
            entity.DefaultValueForCell = viewModel.DefaultValueForCell;
            entity.Cells = viewModel.Cells;
            entity.DataType = viewModel.DataType;

            await db.SaveChangesAsync().ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync().ConfigureAwait(false);
            throw;
        }

        await edit.CommitAsync().ConfigureAwait(false);

        return true;
    }

    protected override async Task<bool> DoDeleteAsync(ColumnEntity item)
    {
        if (string.IsNullOrEmpty(Path))
        {
            return false;
        }

        // A removed column takes the values under it out of the table with it, so those values are
        // remembered as well: putting the column back has to put them back too.
        await using var edit = history.BeginEdit(Path, TableEditKind.ColumnsChanged, "Deleted a column");
        await edit.CaptureBeforeAsync(TableRegion.Columns([item.Id])).ConfigureAwait(false);

        await using var db = await CreateDbAsync().ConfigureAwait(false);
        await using var transaction = await db.Database.BeginTransactionAsync().ConfigureAwait(false);

        try
        {
            var column = await db.Columns
                .FirstOrDefaultAsync(c => c.Id == item.Id)
                .ConfigureAwait(false);

            if (column is null)
            {
                return false;
            }

            // A table has to keep a column for its rows to be read by, so the last one is kept back
            // rather than removed.
            var remaining = await db.Columns
                .AsNoTracking()
                .CountAsync()
                .ConfigureAwait(false);

            if (remaining <= 1)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                Reject("A table needs at least one column.");
                return false;
            }

            db.Columns.Remove(column);
            
            await db.SaveChangesAsync().ConfigureAwait(false);

            await db.Database.ExecuteSqlRawAsync(
                """
                WITH Ordered AS (
                    SELECT 
                        ID,
                        ROW_NUMBER() OVER (ORDER BY ORDINAL_POSITION) AS RN
                    FROM COLUMNS
                )
                UPDATE COLUMNS
                SET ORDINAL_POSITION = (
                    SELECT RN
                    FROM Ordered
                    WHERE Ordered.ID = COLUMNS.ID
                );
                """).ConfigureAwait(false);
            
            await transaction.CommitAsync().ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync().ConfigureAwait(false);
            throw;
        }

        await edit.CommitAsync().ConfigureAwait(false);

        return true;
    }

    /// <summary>
    ///     Moves a column one place along the table, exchanging it with the column beside it.
    /// </summary>
    /// <param name="item">The column to move.</param>
    /// <param name="offset">
    ///     Which way to move it: an offset below zero towards the front of the table, one above zero
    ///     towards the end.
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the move.</param>
    /// <returns><see langword="true" /> when the column changed places.</returns>
    public async Task<bool> MoveAsync(ColumnEntity item, int offset, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(Path) || item is null || offset == 0)
        {
            return false;
        }

        // A move changes only the places the columns sit in, and everything the table holds is read by a
        // column's place, so the whole set of columns is remembered: the set is what the move rearranges.
        await using var edit = history.BeginEdit(Path, TableEditKind.ColumnsChanged, "Moved a column");
        await edit.CaptureBeforeAsync(TableRegion.Columns(), cancellationToken).ConfigureAwait(false);

        await using var db = await CreateDbAsync().ConfigureAwait(false);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var moving = await db.Columns
                .AsNoTracking()
                .FirstOrDefaultAsync(column => column.Id == item.Id, cancellationToken)
                .ConfigureAwait(false);

            if (moving is null)
            {
                return false;
            }

            // The neighbour is found by place rather than by adding one, because the places columns hold
            // are not promised to run without gaps.
            var neighbour = offset > 0
                ? await db.Columns
                    .AsNoTracking()
                    .Where(column => column.OrdinalPosition > moving.OrdinalPosition)
                    .OrderBy(column => column.OrdinalPosition)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false)
                : await db.Columns
                    .AsNoTracking()
                    .Where(column => column.OrdinalPosition < moving.OrdinalPosition)
                    .OrderByDescending(column => column.OrdinalPosition)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);

            if (neighbour is null)
            {
                // The column is already at the edge of the table it can move towards.
                return false;
            }

            // Both places are exchanged in one statement so the columns are never left sharing a place,
            // even for the moment the transaction lasts.
            await db.Database.ExecuteSqlRawAsync(
                """
                UPDATE COLUMNS
                SET ORDINAL_POSITION = CASE ID
                    WHEN @movingId THEN @neighbourOrdinal
                    WHEN @neighbourId THEN @movingOrdinal
                END
                WHERE ID IN (@movingId, @neighbourId);
                """,
                [
                    new SqliteParameter("@movingId", moving.Id),
                    new SqliteParameter("@neighbourId", neighbour.Id),
                    new SqliteParameter("@movingOrdinal", moving.OrdinalPosition),
                    new SqliteParameter("@neighbourOrdinal", neighbour.OrdinalPosition),
                ],
                cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }

        await edit.CommitAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }

    /// <summary>
    ///     Adds a column beside an existing one, holding the same kind of value and what it holds.
    /// </summary>
    /// <param name="source">The column to copy.</param>
    /// <param name="cancellationToken">Token used to cancel the copy.</param>
    /// <returns>The column that was added, or <see langword="null" /> when there was nothing to copy.</returns>
    public async Task<ColumnEntity?> DuplicateAsync(ColumnEntity source, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(Path) || source is null)
        {
            return null;
        }

        await using var db = await CreateDbAsync().ConfigureAwait(false);

        var original = await db.Columns
            .AsNoTracking()
            .FirstOrDefaultAsync(column => column.Id == source.Id, cancellationToken)
            .ConfigureAwait(false);

        if (original is null)
        {
            return null;
        }

        var names = await db.Columns
            .AsNoTracking()
            .Select(column => column.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var name = UniqueName(original.Name, names);

        // The copy is given its id before anything is written, so the values it will hold can be named by
        // the step that records the copy. A step describes the columns by what they are and reads the
        // values under one by the column's id, and nothing would know the id of a column the store has
        // yet to hand one out.
        var highestId = await db.Columns
            .AsNoTracking()
            .Select(column => (int?)column.Id)
            .MaxAsync(cancellationToken)
            .ConfigureAwait(false);

        var copyId = (highestId ?? 0) + 1;
        var copyOrdinal = original.OrdinalPosition + 1;

        // The column the copy will take is named when the table is read before the copy is made. A column
        // that is not there yet holds nothing, and the same region read again afterwards finds it holding
        // what the copy took.
        await using var edit = history.BeginEdit(
            Path,
            TableEditKind.ColumnsChanged,
            $"Duplicated the column '{original.Name}'");

        await edit.CaptureBeforeAsync(TableRegion.Columns([copyId]), cancellationToken).ConfigureAwait(false);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // The columns after the one being copied make room for the copy, so it lands beside the column
            // it was taken from rather than at the end of the table.
            await db.Database.ExecuteSqlRawAsync(
                "UPDATE COLUMNS SET ORDINAL_POSITION = ORDINAL_POSITION + 1 WHERE ORDINAL_POSITION > @ordinal;",
                [new SqliteParameter("@ordinal", original.OrdinalPosition)],
                cancellationToken).ConfigureAwait(false);

            await db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO COLUMNS (ID, NAME, DATA_TYPE, ORDINAL_POSITION, DEFAULT_VALUE_FOR_CELL)
                VALUES (@id, @name, @dataType, @ordinal, @defaultValue);
                """,
                [
                    new SqliteParameter("@id", copyId),
                    new SqliteParameter("@name", name),
                    new SqliteParameter("@dataType", (int)original.DataType),
                    new SqliteParameter("@ordinal", copyOrdinal),
                    new SqliteParameter("@defaultValue", (object?)original.DefaultValueForCell ?? DBNull.Value),
                ],
                cancellationToken).ConfigureAwait(false);

            // The copy holds what the column it was taken from holds, written in one statement so a wide
            // table does not have to be carried through memory to copy one of its columns.
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO CELLS (ROW_ID, COLUMN_ID, VALUE) SELECT ROW_ID, @copyId, VALUE FROM CELLS WHERE COLUMN_ID = @sourceId;",
                [
                    new SqliteParameter("@copyId", copyId),
                    new SqliteParameter("@sourceId", original.Id),
                ],
                cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }

        await edit.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new ColumnEntity
        {
            Id = copyId,
            Name = name,
            DataType = original.DataType,
            DefaultValueForCell = original.DefaultValueForCell,
            OrdinalPosition = copyOrdinal,
        };
    }

    /// <summary>
    ///     Picks a name for a column that the table does not already carry, so a copy can be told apart
    ///     from the column it was taken from.
    /// </summary>
    private static string UniqueName(string name, IReadOnlyCollection<string> taken)
    {
        var baseName = string.IsNullOrWhiteSpace(name) ? "Column" : name.Trim();
        var candidate = $"{baseName} copy";

        if (!taken.Contains(candidate))
        {
            return candidate;
        }

        for (var suffix = 2; ; suffix++)
        {
            var numbered = $"{baseName} copy {suffix}";

            if (!taken.Contains(numbered))
            {
                return numbered;
            }
        }
    }

    /// <summary>
    ///     Picks a name for a new column that the table does not already carry, so a column that was added
    ///     before it was named still reads as a column of the table.
    /// </summary>
    private static string DefaultName(IReadOnlyCollection<string> taken)
    {
        for (var suffix = 1; ; suffix++)
        {
            var candidate = $"Column {suffix}";

            if (!taken.Contains(candidate))
            {
                return candidate;
            }
        }
    }

    private void Reject(string reason)
    {
        EditRejected?.Invoke(this, new ColumnEditRejectedEventArgs(reason));
    }
}
