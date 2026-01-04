using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ModelFlow.DataVirtualization.DataManagement;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Models.TableStore;
using IFactory = TableConverter.Utilities.Database.Interfaces.IDbContextFactory<
    TableConverter.Utilities.Database.Contexts.TableStoreDbContext>;

namespace TableConverter.Services.DataSources;

public class TableStoreSearchResultDataSource : DataSource<SearchResult>
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

    public TableStoreSearchResultDataSource(IFactory dbContextFactory) 
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

    protected override async Task<bool> ContainsAsync(SearchResult item)
    {
        if (string.IsNullOrEmpty(Path))
        {
            return false;
        }
        
        if (item is null || item.RowId < 0 || item.ColumnId < 0)
        {
            return false;
        }

        await using var db = await CreateDbAsync().ConfigureAwait(false);
        
        return await db.SearchResults
            .AsNoTracking()
            .AnyAsync(sr => sr.RowId == item.RowId && sr.ColumnId == item.ColumnId)
            .ConfigureAwait(false);
    }

    protected override async Task<int> GetCountAsync(Func<IQueryable<SearchResult>, IQueryable<SearchResult>> filterQuery)
    {
        if (string.IsNullOrEmpty(Path))
        {
            return 0;
        }
        
        await using var db = await CreateDbAsync().ConfigureAwait(false);
        
        var query = db.SearchResults.AsNoTracking();

        query = filterQuery(query);

        return await query.CountAsync().ConfigureAwait(false);
    }

    protected override async Task<IEnumerable<SearchResult>> GetItemsAtAsync(
        int offset,
        int count,
        Func<IQueryable<SearchResult>, IQueryable<SearchResult>> filterSortQuery)
    {
        if (string.IsNullOrEmpty(Path))
        {
            return [];
        }
        
        await using var db = await CreateDbAsync().ConfigureAwait(false);

        IQueryable<SearchResult> query = db.SearchResults.AsNoTracking();

        query = filterSortQuery(query);

        return await query
            .Skip(offset)
            .Take(count)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public override async Task<SearchResult?> GetItemAsync(Expression<Func<SearchResult, bool>> predicate)
    {
        if (string.IsNullOrEmpty(Path))
        {
            return null;
        }
        
        await using var db = await CreateDbAsync().ConfigureAwait(false);
        
        return await db.SearchResults
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate)
            .ConfigureAwait(false);
    }

    protected override SearchResult GetPlaceHolder(int index, int page, int offset)
    {
        return new SearchResult
        {
            RowId = index,
            ColumnId = 0,
            Value = "...",
        };
    }

    protected override bool ModelsEqual(SearchResult a, SearchResult b)
    {
        return a.RowId == b.RowId && a.ColumnId == b.ColumnId && a.Value == b.Value && a.FoundValue == b.FoundValue;
    }

    protected override Task<bool> DoCreateAsync(SearchResult item)
    {
        throw new NotSupportedException();
    }

    protected override Task<bool> DoUpdateAsync(SearchResult viewModel)
    {
        throw new NotSupportedException();
    }

    protected override Task<bool> DoDeleteAsync(SearchResult item)
    {
        throw new NotSupportedException();
    }
}