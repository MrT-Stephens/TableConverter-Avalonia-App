using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ModelFlow.DataVirtualization.DataManagement;
using TableConverter.Services.DataSources.Base;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.History;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Utilities.Extensions;
using TableConverter.Utilities.Database.Interfaces;

namespace TableConverter.Services.DataSources;

public class TableStoreDataSource(
    ITableStoreDbContextFactory databaseContextFactory,
    ITableHistory history)
    : DataSourceFromPath<RowEntity>(databaseContextFactory, 250, 5)
{
    private int? _ColumnCount;
    public int? ColumnCount
    {
        get => _ColumnCount;
        set
        {
            _ColumnCount = value;
            OnPropertyChanged();
        }
    }

    protected override async Task<bool> ContainsAsync(RowEntity item)
    {
        if (string.IsNullOrEmpty(Path))
        {
            return false;
        }
        
        if (item is null || item.Id <= 0)
        {
            return false;
        }

        await using var db = await CreateDbAsync().ConfigureAwait(false);

        return await db.Rows
            .AsNoTracking()
            .AnyAsync(r => r.Id == item.Id)
            .ConfigureAwait(false);
    }

    protected override async Task<int> GetCountAsync(Func<IQueryable<RowEntity>, IQueryable<RowEntity>> filterQuery)
    {
        if (string.IsNullOrEmpty(Path))
        {
            return 0;
        }
        
        await using var db = await CreateDbAsync().ConfigureAwait(false);
        
        ColumnCount = await db.Columns
            .AsNoTracking()
            .CountAsync()
            .ConfigureAwait(false);
        
        var query = db.Rows.AsNoTracking();

        query = filterQuery(query);

        return await query.CountAsync().ConfigureAwait(false);
    }

    protected override async Task<IEnumerable<RowEntity>> GetItemsAtAsync(
        int offset,
        int count,
        Func<IQueryable<RowEntity>, IQueryable<RowEntity>> filterSortQuery)
    {
        if (string.IsNullOrEmpty(Path))
        {
            return [];
        }
        
        await using var db = await CreateDbAsync().ConfigureAwait(false);

        IQueryable<RowEntity> query = db.Rows
            .AsNoTracking()
            .AsSplitQuery()
            .Include(r => r.Cells);

        query = filterSortQuery(query);

        // The order the rows are read in is the store's own order, which is what makes a row's place in
        // the table its identity: the grid shows the nth row it is given, and the nth row of the store is
        // the one carrying the nth id.
        var rows = await query
            .OrderBy(row => row.Id)
            .Skip(offset)
            .Take(count)
            .ToListAsync()
            .ConfigureAwait(false);

        // A cell is shown where its column sits, not where the cell's own id falls, and a column that has
        // been moved keeps its id while changing its place. The store hands the cells of a row back in the
        // order it keeps them in, which is decided by their ids, so they are put in the order the grid reads
        // them in here: without this, moving a column would move the headings but leave the values behind
        // and every value would show under the wrong one.
        var ordinalByColumnId = await db.Columns
            .AsNoTracking()
            .Select(column => new { column.Id, column.OrdinalPosition })
            .ToDictionaryAsync(column => column.Id, column => column.OrdinalPosition)
            .ConfigureAwait(false);

        foreach (var row in rows)
        {
            // A cell of a column that is no longer there is put at the end rather than thrown away, so a
            // value the table still holds is never hidden by a column set this read did not see.
            row.Cells = new ObservableCollection<CellEntity>(
                row.Cells.OrderBy(cell => ordinalByColumnId.GetValueOrDefault(cell.ColumnId, int.MaxValue)));
        }

        return rows;
    }

    public override async Task<RowEntity?> GetItemAsync(Expression<Func<RowEntity, bool>> predicate)
    {
        if (string.IsNullOrEmpty(Path))
        {
            return null;
        }
        
        await using var db = await CreateDbAsync().ConfigureAwait(false);
        
        return await db.Rows
            .AsNoTracking()
            .Include(r => r.Cells)
            .ThenInclude(c => c.Column)
            .FirstOrDefaultAsync(predicate)
            .ConfigureAwait(false);
    }

    protected override RowEntity GetPlaceHolder(int index, int page, int offset)
    {
        if (string.IsNullOrEmpty(Path))
        {
            return new RowEntity();
        }
        
        var row = new RowEntity
        {
            Id = index + 1,
        };

        for (var i = 0; i < ColumnCount; i++)
        {
            row.Cells.Add(new CellEntity
            {
                RowId = index,
                ColumnId = i + 1,
                Value = DataSourcePlaceholder.Text
            });
        }

        return row;
    }

    protected override bool ModelsEqual(RowEntity a, RowEntity b) => a.Id == b.Id;

    protected override async Task<bool> DoCreateAsync(RowEntity item)
    {
        if (string.IsNullOrEmpty(Path))
        {
            return false;
        }

        // An edit made straight in the grid is a step in the table's history like any other, so it is
        // described the same way the commands describe theirs.
        await using var edit = history.BeginEdit(Path, TableEditKind.RowsAdded, "Added a row");

        await using var db = await CreateDbAsync().ConfigureAwait(false);
        await using var transaction = await db.Database.BeginTransactionAsync().ConfigureAwait(false);

        try
        {
            await db.Rows.AddAsync(item).ConfigureAwait(false);
            
            await db.SaveChangesAsync().ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync().ConfigureAwait(false);
            throw;
        }

        // A row only has the id it is remembered under once the store has given it one, so it is described
        // now that it has.
        await edit.CaptureAfterAsync(TableRegion.Rows([item.Id])).ConfigureAwait(false);
        await edit.CommitAsync().ConfigureAwait(false);

        return true;
    }

    protected override async Task<bool> DoUpdateAsync(RowEntity viewModel)
    {
        if (string.IsNullOrEmpty(Path))
        {
            return false;
        }

        // The whole row is described before it is written, because a cell the user edited is not reported
        // to the data source - only the row it belongs to is - and the diff works out which values moved.
        await using var edit = history.BeginEdit(Path, TableEditKind.CellsChanged, "Edited a row");
        await edit.CaptureBeforeAsync(TableRegion.Rows([viewModel.Id])).ConfigureAwait(false);

        await using var db = await CreateDbAsync().ConfigureAwait(false);
        await using var transaction = await db.Database.BeginTransactionAsync().ConfigureAwait(false);

        try
        {
            var entity = await db.Rows
                .Include(r => r.Cells)
                .FirstOrDefaultAsync(r => r.Id == viewModel.Id)
                .ConfigureAwait(false);

            if (entity is null)
            {
                return false;
            }

            // Only the values of the row's cells are written back. Each cell is matched to the one the
            // store already holds by the column it belongs to rather than replacing the row's cells
            // wholesale, which would try to insert cells the store already has and collide on their
            // keys.
            var storedCells = entity.Cells
                .GroupBy(cell => cell.ColumnId)
                .ToDictionary(group => group.Key, group => group.First());

            foreach (var cell in viewModel.Cells)
            {
                if (storedCells.TryGetValue(cell.ColumnId, out var stored))
                {
                    stored.Value = cell.Value;
                    continue;
                }

                // A column added since the row was read has no cell of the row's to write to yet.
                entity.Cells.Add(new CellEntity
                {
                    RowId = entity.Id,
                    ColumnId = cell.ColumnId,
                    Value = cell.Value
                });
            }

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

    protected override async Task<bool> DoDeleteAsync(RowEntity item)
    {
        if (string.IsNullOrEmpty(Path))
        {
            return false;
        }

        await using var edit = history.BeginEdit(Path, TableEditKind.RowsDeleted, "Deleted a row");
        await edit.CaptureBeforeAsync(TableRegion.Rows([item.Id])).ConfigureAwait(false);

        await using var db = await CreateDbAsync().ConfigureAwait(false);
        await using var transaction = await db.Database.BeginTransactionAsync().ConfigureAwait(false);

        try
        {
            var entity = await db.Rows
                .Include(r => r.Cells)
                .FirstOrDefaultAsync(r => r.Id == item.Id)
                .ConfigureAwait(false);

            if (entity is null)
            {
                return false;
            }

            db.Rows.Remove(entity);
            
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
}