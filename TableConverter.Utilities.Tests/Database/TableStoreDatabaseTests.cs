using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Extensions;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities.Tests.Database;

public class TableStoreDatabaseTests
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddSingleton<IEventManager, EventManager>();
        services.AddTableStoreDatabase();

        return services.BuildServiceProvider();
    }

    [Fact]
    public void AddTableStoreDatabase_Registers_EfCore_Factory_And_PathAware_Factory()
    {
        using var provider = BuildProvider();

        var efFactory = provider.GetService<IDbContextFactory<TableStoreDbContext>>();
        var pathAwareFactory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        Assert.NotNull(efFactory);
        Assert.Same(efFactory, pathAwareFactory);
    }

    [Fact]
    public async Task CreateDbContextAsync_Creates_Database_At_Path_And_Derives_SourceId()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.tcstore");

        try
        {
            await using var context = await factory.CreateDbContextAsync(path);

            Assert.True(File.Exists(path), "the SQLite file should have been created");
            Assert.Equal(GuidUtility.Create(GuidUtility.UrlNamespace, path), context.SourceId);
        }
        finally
        {
            foreach (var file in new[] { path, $"{path}-wal", $"{path}-shm" })
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }
    }

    [Fact]
    public async Task CreateDbContextAsync_Keeps_Paths_Isolated()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var firstPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.tcstore");
        var secondPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.tcstore");

        try
        {
            await using (var context = await factory.CreateDbContextAsync(firstPath))
            {
                await context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO COLUMNS (NAME, DATA_TYPE, ORDINAL_POSITION) VALUES ('first', 0, 1);");
            }

            await using (var context = await factory.CreateDbContextAsync(secondPath))
            {
                Assert.Equal(0, await context.Columns.CountAsync());
                Assert.Equal(GuidUtility.Create(GuidUtility.UrlNamespace, secondPath), context.SourceId);
            }

            await using (var context = await factory.CreateDbContextAsync(firstPath))
            {
                Assert.Equal(1, await context.Columns.CountAsync());
                Assert.Equal(GuidUtility.Create(GuidUtility.UrlNamespace, firstPath), context.SourceId);
            }
        }
        finally
        {
            DeleteStore(firstPath);
            DeleteStore(secondPath);
        }
    }

    [Fact]
    public async Task CreateDbContextAsync_Returns_Contexts_Bound_To_The_Same_Store()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.tcstore");

        try
        {
            // Two overlapping contexts for one store must be distinct instances that agree on
            // which store they are bound to.
            await using var first = await factory.CreateDbContextAsync(path);
            await using var second = await factory.CreateDbContextAsync(path);

            Assert.NotSame(first, second);
            Assert.Equal(path, first.Path);
            Assert.Equal(first.Path, second.Path);
            Assert.Equal(first.SourceId, second.SourceId);
            Assert.Equal(GuidUtility.Create(GuidUtility.UrlNamespace, path), first.SourceId);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task CreateDbContextAsync_Allows_Parallel_Contexts_For_One_Store()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.tcstore");
        var expectedSourceId = GuidUtility.Create(GuidUtility.UrlNamespace, path);

        try
        {
            var mismatches = 0;

            await Parallel.ForEachAsync(
                Enumerable.Range(0, 8),
                async (_, cancellationToken) =>
                {
                    await using var context = await factory.CreateDbContextAsync(path, cancellationToken);

                    if (context.SourceId != expectedSourceId || context.Path != path)
                    {
                        Interlocked.Increment(ref mismatches);
                    }

                    Assert.Equal(0, await context.Columns.AsNoTracking().CountAsync(cancellationToken));
                });

            Assert.Equal(0, mismatches);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task CreateDbContextAsync_Keeps_Working_When_Many_Stores_Are_Opened()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        // More stores than the factory retains pools for, so this exercises trimming.
        var paths = Enumerable.Range(0, 100)
            .Select(_ => Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.tcstore"))
            .ToList();

        try
        {
            foreach (var path in paths)
            {
                await using var context = await factory.CreateDbContextAsync(path);

                Assert.Equal(path, context.Path);
                Assert.Equal(GuidUtility.Create(GuidUtility.UrlNamespace, path), context.SourceId);
                Assert.True(File.Exists(path));
            }

            // Re-opening a store that may have been evicted must still work.
            await using var reopened = await factory.CreateDbContextAsync(paths[0]);

            Assert.Equal(paths[0], reopened.Path);
            Assert.Equal(0, await reopened.Columns.CountAsync());
        }
        finally
        {
            foreach (var path in paths)
            {
                DeleteStore(path);
            }
        }
    }

    [Fact]
    public void Lookup_On_A_Context_Created_Without_The_Extension_Fails_With_A_Clear_Error()
    {
        var options = new DbContextOptionsBuilder<TableStoreDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            var context = new TableStoreDbContext(options);
            _ = context.SourceId;
        });

        Assert.Contains("UseTableStore", exception.Message);
    }

    private static void DeleteStore(string path)
    {
        foreach (var file in new[] { path, $"{path}-wal", $"{path}-shm" })
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }
}

