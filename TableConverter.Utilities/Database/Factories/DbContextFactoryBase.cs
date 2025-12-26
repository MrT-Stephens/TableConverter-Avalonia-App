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

        // Ensures tables/indexes exist based on your model:
        db.Database.EnsureCreated();
        
        // Optional but commonly helpful for SQLite:
        db.Database.ExecuteSqlRaw("PRAGMA foreign_keys = ON;");
        
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

        // Ensures tables/indexes exist based on your model.
        await db.Database.EnsureCreatedAsync(cancellationToken);

        // Optional but commonly helpful for SQLite:
        await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;", cancellationToken);

        return db;
    }
}