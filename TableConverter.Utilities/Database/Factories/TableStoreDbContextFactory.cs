using System.Collections.Concurrent;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TableConverter.Utilities.Database.Configuration;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities.Database.Factories;

/// <summary>
/// Creates <see cref="TableStoreDbContext"/> instances for a specific table store data source.
/// </summary>
/// <remarks>
/// <para>
/// The provider (SQLite on desktop, the in-memory provider in the browser) and the shared options
/// (logging, interceptors) are configured once through EF Core's <c>AddDbContextFactory</c>. EF Core
/// factories are bound to a single set of options, so this factory keeps one EF Core
/// <see cref="PooledDbContextFactory{TContext}"/> per table store and lets EF Core create the
/// contexts; the identity of the table store travels in the options instead of the constructor.
/// </para>
/// <para>
/// Stores are tracked only so that the contexts EF Core pools for them can be reused. Dropping a
/// store is harmless (contexts that were already handed out keep working, and a later request just
/// gets a fresh pool), so the bookkeeping is capped to keep a long lived session that opens many
/// documents bounded.
/// </para>
/// </remarks>
public sealed class TableStoreDbContextFactory(
    DbContextOptions<TableStoreDbContext> options,
    IEventManager eventManager) : ITableStoreDbContextFactory
{
    /// <summary>
    /// Upper bound on the number of context instances EF Core keeps alive per table store.
    /// </summary>
    private const int MaxPooledContexts = 32;

    /// <summary>
    /// Upper bound on the number of table stores whose pools are retained.
    /// </summary>
    private const int MaxTrackedStores = 64;

    private readonly bool _useInMemoryProvider = OperatingSystem.IsBrowser();

    private readonly ConcurrentDictionary<string, StoreEntry> _stores = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public TableStoreDbContext CreateDbContext()
        => throw new NotSupportedException(
            "A table store path is required. Use CreateDbContext(string path) instead.");

    /// <inheritdoc />
    public Task<TableStoreDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        => Task.FromException<TableStoreDbContext>(new NotSupportedException(
            "A table store path is required. Use CreateDbContextAsync(string path, ...) instead."));

    /// <inheritdoc />
    public TableStoreDbContext CreateDbContext(string path)
    {
        var store = GetStore(path);

        var dbContext = store.Factory.CreateDbContext();

        // Creating a store issues DDL, so first use is serialised per path; afterwards this is a
        // cheap no-op check.
        store.CreationLock.Wait();
        try
        {
            dbContext.Database.EnsureCreated();
        }
        finally
        {
            store.CreationLock.Release();
        }

        return dbContext;
    }

    /// <inheritdoc />
    public async Task<TableStoreDbContext> CreateDbContextAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        var store = GetStore(path);

        var dbContext = await store.Factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        await store.CreationLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            store.CreationLock.Release();
        }

        return dbContext;
    }

    private StoreEntry GetStore(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("A table store path must be provided.", nameof(path));
        }

        var store = _stores.GetOrAdd(path, BuildStore);
        store.LastUsed = Environment.TickCount64;

        TrimTrackedStores();

        return store;
    }

    private StoreEntry BuildStore(string path) => new(BuildFactory(path));

    private PooledDbContextFactory<TableStoreDbContext> BuildFactory(string path)
        => new(BuildOptions(path), MaxPooledContexts);

    private void TrimTrackedStores()
    {
        var excess = _stores.Count - MaxTrackedStores;

        if (excess <= 0)
        {
            return;
        }

        foreach (var (path, store) in _stores.OrderBy(entry => entry.Value.LastUsed).Take(excess))
        {
            // Never drop a store that another caller is still creating.
            if (store.CreationLock.CurrentCount == 0)
            {
                continue;
            }

            _stores.TryRemove(path, out _);
        }
    }

    private DbContextOptions<TableStoreDbContext> BuildOptions(string path)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TableStoreDbContext>(options);

        optionsBuilder.UseTableStore(path, eventManager);

        if (_useInMemoryProvider)
        {
            // WASM has no native SQLite, so the data lives in memory for the lifetime of the process.
            optionsBuilder.UseInMemoryDatabase(path);
        }
        else
        {
            // Foreign keys are turned on through the connection string rather than a hand written
            // PRAGMA statement; the remaining pragmas come from SqlitePragmaConnectionInterceptor.
            optionsBuilder.UseSqlite(new SqliteConnectionStringBuilder
            {
                DataSource = path,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Pooling = true,
                ForeignKeys = true,
            }.ToString());
        }

        return optionsBuilder.Options;
    }

    private sealed class StoreEntry(PooledDbContextFactory<TableStoreDbContext> factory)
    {
        public PooledDbContextFactory<TableStoreDbContext> Factory { get; } = factory;

        public SemaphoreSlim CreationLock { get; } = new(1, 1);

        public long LastUsed { get; set; }
    }
}
