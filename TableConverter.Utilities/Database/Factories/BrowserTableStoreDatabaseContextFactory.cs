using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Interfaces;
using TableConverter.Utilities;

namespace TableConverter.Utilities.Database.Factories;

/// <summary>
/// Browser-friendly database context factory that uses EF Core InMemory provider.
/// This avoids native SQLite dependencies which are not available in WASM.
/// </summary>
public sealed class BrowserTableStoreDatabaseContextFactory(IEventManager eventManager, ILoggerFactory loggerFactory)
    : Interfaces.IDatabaseContextFactory<TableStoreDbContext>
{
    public TableStoreDbContext Create(string path)
    {
        var dbName = string.IsNullOrWhiteSpace(path) ? Guid.NewGuid().ToString() : path;

        var options = new DbContextOptionsBuilder<TableStoreDbContext>()
            .UseInMemoryDatabase(dbName)
            .UseLoggerFactory(loggerFactory)
            .Options;

        var sourceId = GuidUtility.Create(GuidUtility.UrlNamespace, dbName);

        var db = new TableStoreDbContext(options, eventManager, sourceId);

        // Ensure database created (InMemory provider is ready immediately)
        db.Database.EnsureCreated();

        return db;
    }

    public Task<TableStoreDbContext> CreateAsync(string path, CancellationToken cancellationToken = default)
    {
        var db = Create(path);
        return Task.FromResult(db);
    }
}


