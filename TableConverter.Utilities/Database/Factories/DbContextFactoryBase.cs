using Microsoft.EntityFrameworkCore;

namespace TableConverter.Utilities.Database.Factories;

public abstract class DbContextFactoryBase<TDbContext>(Func<DbContextOptions<TDbContext>, TDbContext> factory)
    : Interfaces.IDbContextFactory<TDbContext> where TDbContext : DbContext
{
    public TDbContext Create(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("SQLite DB path must be provided.", nameof(path));

        var connectionString = $"Data Source={path};";
        
        var options = new DbContextOptionsBuilder<TDbContext>()
            .UseSqlite(connectionString)
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging(false)
            .Options;
        
        var db = factory(options);
        
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
            throw new ArgumentException("SQLite DB path must be provided.", nameof(path));

        var connectionString = $"Data Source={path};";

        var options = new DbContextOptionsBuilder<TDbContext>()
            .UseSqlite(connectionString)
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging(false)
            .Options;

        var db = factory(options);
        
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