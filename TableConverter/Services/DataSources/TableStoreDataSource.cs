using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ModelFlow.DataVirtualization.DataManagement;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Models;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Utilities.Extensions;
using IFactory = TableConverter.Utilities.Database.Interfaces.IDbContextFactory<
    TableConverter.Utilities.Database.Contexts.TableStoreDbContext>;

namespace TableConverter.Services.DataSources;

public class TableStoreDataSource : DataSource<RowEntity>
{
    private readonly IFactory _dbContextFactory;
    private readonly string _path;

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

    public TableStoreDataSource(IFactory dbContextFactory, string path) : base(250, 5)
    {
        _dbContextFactory = dbContextFactory 
            ?? throw new ArgumentNullException(nameof(dbContextFactory));
        
        _path = string.IsNullOrWhiteSpace(path) 
            ? throw new ArgumentException("Path must be provided.", nameof(path)) 
            : path;
    }

    private async Task<TableStoreDbContext> CreateDbAsync()
    {
        var db = await _dbContextFactory.CreateAsync(_path);
        return db;
    }
    
    private TableStoreDbContext CreateDb()
    {
        return _dbContextFactory.Create(_path);
    }

    protected override async Task<bool> ContainsAsync(RowEntity item)
    {
        if (item is null || item.RowId <= 0)
        {
            return false;
        }

        await using var db = await CreateDbAsync().ConfigureAwait(false);

        return await db.Rows
            .AsNoTracking()
            .AnyAsync(r => r.RowId == item.RowId)
            .ConfigureAwait(false);
    }

    protected override async Task<int> GetCountAsync(Func<IQueryable<RowEntity>, IQueryable<RowEntity>> filterQuery)
    {
        await using var db = await CreateDbAsync().ConfigureAwait(false);
        
        var query = db.Rows.AsNoTracking();

        query = filterQuery(query);

        return await query.CountAsync().ConfigureAwait(false);
    }

    protected override async Task<IEnumerable<RowEntity>> GetItemsAtAsync(
        int offset,
        int count,
        Func<IQueryable<RowEntity>, IQueryable<RowEntity>> filterSortQuery)
    {
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
        if (ColumnCount is null)
        {
            using var db = CreateDb();
            ColumnCount = db.Columns.Count();
        }
        
        var row = new RowEntity
        {
            RowId = index,
        };

        for (var i = 0; i < ColumnCount; i++)
        {
            row.Cells.Add(new CellEntity
            {
                RowId = index,
                ColumnId = i,
                Value = "..."
            });
        }

        return row;
    }

    protected override bool ModelsEqual(RowEntity a, RowEntity b)
        => a.RowId == b.RowId;

    protected override async Task<bool> DoCreateAsync(RowEntity item)
    {
        await using var db = await CreateDbAsync().ConfigureAwait(false);
        
        await db.Rows.AddAsync(item).ConfigureAwait(false);
        await db.SaveChangesAsync().ConfigureAwait(false);
        
        return true;
    }

    protected override async Task<bool> DoUpdateAsync(RowEntity viewModel)
    {
        await using var db = await CreateDbAsync().ConfigureAwait(false);
        
        var entity = await db.Rows
            .Include(r => r.Cells)
            .FirstOrDefaultAsync(r => r.RowId == viewModel.RowId)
            .ConfigureAwait(false);
        
        if (entity is null)
        {
            return false;
        }
        
        entity.RowId = viewModel.RowId;
        
        entity.Cells.Clear();
        entity.Cells.AddRange(viewModel.Cells);
        
        await db.SaveChangesAsync().ConfigureAwait(false);
        
        return true;
    }

    protected override async Task<bool> DoDeleteAsync(RowEntity item)
    {
        await using var db = await CreateDbAsync().ConfigureAwait(false);
        
        var entity = await db.Rows
            .Include(r => r.Cells)
            .FirstOrDefaultAsync(r => r.RowId == item.RowId)
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