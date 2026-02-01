using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TableConverter.Services.DataSources.Base;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Models.TableStore;
using IFactory = TableConverter.Utilities.Database.Interfaces.IDatabaseContextFactory<
    TableConverter.Utilities.Database.Contexts.TableStoreDbContext>;

namespace TableConverter.Services.DataSources;

public class TableStoreColumnsDataSource(IFactory databaseContextFactory)
    : DataSourceFromPath<ColumnEntity, TableStoreDbContext>(databaseContextFactory, 250, 5)
{
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
            Name = "...",
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

        await using var db = await CreateDbAsync().ConfigureAwait(false);

        var max = await db.Columns
            .AsNoTracking()
            .MaxAsync(c => c.OrdinalPosition)
            .ConfigureAwait(false);

        item.OrdinalPosition = max + 1;

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
            .FirstOrDefaultAsync(r => r.Id == viewModel.Id)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return false;
        }

        entity.Name = viewModel.Name;
        entity.DefaultValueForCell = viewModel.DefaultValueForCell;
        entity.Cells = viewModel.Cells;
        entity.DataType = viewModel.DataType;

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

        var column = await db.Columns
            .Include(c => c.Cells)
            .FirstOrDefaultAsync(c => c.Id == item.Id)
            .ConfigureAwait(false);

        if (column is null)
        {
            return false;
        }

        db.Columns.Remove(column);

        await db.Database.ExecuteSqlRawAsync(
            """
            WITH ORDERED AS (
                SELECT ROWID AS ROW_ID, ROW_NUMBER() OVER (ORDER BY NAME) AS RN
                FROM COLUMNS
            )
            UPDATE COLUMNS
            SET ORDINAL_POSITION = (
                SELECT RN FROM ORDERED WHERE ORDERED.ROW_ID = COLUMNS.ROWID
            );
            """).ConfigureAwait(false);

        await db.SaveChangesAsync().ConfigureAwait(false);

        return true;
    }
}