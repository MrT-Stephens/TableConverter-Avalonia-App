using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TableConverter.Utilities.Database.Extensions;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities.Database.Factories;

public abstract class DatabaseContextFactoryBase<TDbContext>(Func<DbContextOptions<TDbContext>, IEventManager, Guid, TDbContext> factory, IEventManager eventManager)
    : Interfaces.IDatabaseContextFactory<TDbContext> where TDbContext : DbContext
{
    public TDbContext Create(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("SQLite DB path must be provided.", nameof(path));
        }

        var connectionString = $"Data Source={path};";
        
        var options = new DbContextOptionsBuilder<TDbContext>()
            .UseSqlite(connectionString)
#if DEBUG
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
            .LogTo(s => Debug.WriteLine(s))
#endif
            .Options;

        // Generate unique ID for the source database path (used for tracking).
        var sourceId = GuidUtility.Create(GuidUtility.UrlNamespace, path);
        
        var db = factory(options, eventManager, sourceId);
        
        db.Database.EnsureCreated();
        
        db.Database.ExecuteSqlRaw("""
            PRAGMA foreign_keys = ON;
            PRAGMA journal_mode = WAL;
            PRAGMA synchronous = NORMAL;
            PRAGMA temp_store = MEMORY;
            PRAGMA busy_timeout = 5000;
            """);
        
        return db;
    }

    public async Task<TDbContext> CreateAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("SQLite DB path must be provided.", nameof(path));
        }

        var connectionString = $"Data Source={path};";

        var options = new DbContextOptionsBuilder<TDbContext>()
            .UseSqlite(connectionString)
#if DEBUG
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
            .LogTo(s => Debug.WriteLine(s))
#endif
            .Options;
        
        // Generate unique ID for the source database path (used for tracking).
        var sourceId = GuidUtility.Create(GuidUtility.UrlNamespace, path);

        var db = factory(options, eventManager, sourceId);
        
        await db.Database.EnsureCreatedAsync(cancellationToken);
        
        await db.Database.ExecuteSqlRawAsync("""
            PRAGMA foreign_keys = ON;
            PRAGMA journal_mode = WAL;
            PRAGMA synchronous = NORMAL;
            PRAGMA temp_store = MEMORY;
            PRAGMA busy_timeout = 5000;
            """, cancellationToken);

        return db;
    }
}