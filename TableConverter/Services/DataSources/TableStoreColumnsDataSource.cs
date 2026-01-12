using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ModelFlow.DataVirtualization.DataManagement;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Utilities.Extensions;
using TableConverter.ViewModels.Models;
using IFactory = TableConverter.Utilities.Database.Interfaces.IDbContextFactory<
    TableConverter.Utilities.Database.Contexts.TableStoreDbContext>;

namespace TableConverter.Services.DataSources;

public class TableStoreColumnsDataSource : DataSource<ColumnEntity>
{
    private readonly IFactory _dbContextFactory;

    private string _Path;

    public string Path
    {
        get => _Path;
        set
        {
            _Path = value;
            Invalidate();
            OnPropertyChanged();
        }
    }

    public TableStoreColumnsDataSource(IFactory dbContextFactory)
        : base(250, 5)
    {
        _dbContextFactory = dbContextFactory
            ?? throw new ArgumentNullException(nameof(dbContextFactory));

        Path = string.Empty;
    }

    private Task<TableStoreDbContext> CreateDbAsync()
    {
        return _dbContextFactory.CreateAsync(Path);
    }

    protected override async Task<bool> ContainsAsync(ColumnEntity item)
    {
        if (string.IsNullOrEmpty(Path))
        {
            return false;
        }
        
        if (item is null || item.ColumnId < 0)
        {
            return false;
        }

        await using var db = await CreateDbAsync().ConfigureAwait(false);
        
        return await db.Columns
            .AsNoTracking()
            .AnyAsync(col => col.ColumnId == item.ColumnId)
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
            ColumnId = index,
            Name = "...",
            Ordinal = index + 1,
            DefaultValueForCell = string.Empty
        };
    }

    protected override bool ModelsEqual(ColumnEntity a, ColumnEntity b)
    {
        return a.ColumnId == b.ColumnId 
               && a.Name.Equals(b.Name) 
               && a.Ordinal == b.Ordinal 
               && a.DataType == b.DataType
               && a.DefaultValueForCell == b.DefaultValueForCell;
    }

    protected override async Task<bool> DoCreateAsync(ColumnEntity item)
    {
        if (string.IsNullOrEmpty(Path))
        {
            return false;
        }
        
        await using var db = await CreateDbAsync().ConfigureAwait(false);
        
        await db.Columns.AddAsync(item).ConfigureAwait(false);
        await db.SaveChangesAsync().ConfigureAwait(false);
        
        return true;
    }

    protected override async Task<bool> DoUpdateAsync(ColumnEntity viewModel)
    {
        if (string.IsNullOrEmpty(Path))
        {
            return false;
        }
        
        await using var db = await CreateDbAsync().ConfigureAwait(false);
        
        var entity = await db.Columns
            .FirstOrDefaultAsync(r => r.ColumnId == viewModel.ColumnId)
            .ConfigureAwait(false);
        
        if (entity is null)
        {
            return false;
        }

        entity.MapTo(viewModel, nameof(ColumnEntity.ColumnId));

        await db.SaveChangesAsync().ConfigureAwait(false);

        return true;
    }

    protected override async Task<bool> DoDeleteAsync(ColumnEntity item)
    {
        if (string.IsNullOrEmpty(Path))
        {
            return false;
        }
        
        await using var db = await CreateDbAsync().ConfigureAwait(false);

        var entity = await db.Columns
            .FirstOrDefaultAsync(c => c.ColumnId == item.ColumnId)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return false;
        }

        db.Columns.Remove(entity);
        await db.SaveChangesAsync().ConfigureAwait(false);

        return true;
    }
}