using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TableConverter.Services.DataSources.Base;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Utilities.Extensions;
using IFactory = TableConverter.Utilities.Database.Interfaces.IDbContextFactory<
    TableConverter.Utilities.Database.Contexts.TableStoreDbContext>;

namespace TableConverter.Services.DataSources;

public class TableStoreDataSource(IFactory dbContextFactory)
    : DataSourceFromPath<RowEntity, TableStoreDbContext>(dbContextFactory, 250, 5)
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
        
        ColumnCount = await db.Columns.CountAsync().ConfigureAwait(false);
        
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

        return await query
            .Skip(offset)
            .Take(count)
            .ToListAsync()
            .ConfigureAwait(false);
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
                Value = "..."
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
        
        await using var db = await CreateDbAsync().ConfigureAwait(false);
        
        await db.Rows.AddAsync(item).ConfigureAwait(false);
        await db.SaveChangesAsync().ConfigureAwait(false);
        
        return true;
    }

    protected override async Task<bool> DoUpdateAsync(RowEntity viewModel)
    {
        if (string.IsNullOrEmpty(Path))
        {
            return false;
        }
        
        await using var db = await CreateDbAsync().ConfigureAwait(false);
        
        var entity = await db.Rows
            .Include(r => r.Cells)
            .FirstOrDefaultAsync(r => r.Id == viewModel.Id)
            .ConfigureAwait(false);
        
        if (entity is null)
        {
            return false;
        }
        
        entity.Id = viewModel.Id;
        
        entity.Cells.Clear();
        entity.Cells.AddRange(viewModel.Cells);
        
        await db.SaveChangesAsync().ConfigureAwait(false);
        
        return true;
    }

    protected override async Task<bool> DoDeleteAsync(RowEntity item)
    {
        if (string.IsNullOrEmpty(Path))
        {
            return false;
        }
        
        await using var db = await CreateDbAsync().ConfigureAwait(false);
        
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
        
        return true;
    }
}