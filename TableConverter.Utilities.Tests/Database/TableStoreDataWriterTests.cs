using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.Extensions;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities.Tests.Database;

/// <summary>
/// Covers <see cref="TableStoreDataWriter" />, which is what fills a store with the result of an
/// import.
/// </summary>
public class TableStoreDataWriterTests
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddSingleton<IEventManager, EventManager>();
        services.AddTableStoreDatabase();

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Reads the store back as a flat list of cells ordered by row, then column.
    /// </summary>
    private static async Task<string?[]> ReadCellsAsync(
        ITableStoreDbContextFactory factory,
        string path,
        int columnCount)
    {
        await using var db = await factory.CreateDbContextAsync(path);

        var rows = await db.Rows
            .AsNoTracking()
            .Include(row => row.Cells)
            .OrderBy(row => row.Id)
            .ToListAsync();

        return rows
            .SelectMany(row => Enumerable.Range(1, columnCount)
                .Select(columnId => row.Cells.SingleOrDefault(cell => cell.ColumnId == columnId)?.Value))
            .ToArray();
    }

    private static async Task WriteAsync(ITableStoreDbContextFactory factory, string path, TableData tableData)
    {
        await using var dbContext = await factory.CreateDbContextAsync(path);

        await TableStoreDataWriter.WriteAsync(dbContext, tableData);
    }

    [Fact]
    public async Task WriteAsync_Stores_Headers_Rows_And_Cells()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.tcstore");

        try
        {
            var tableData = new TableData(
                ["A", "B", "C"],
                [
                    ["a1", "b1", "c1"],
                    // A missing middle value and a short row both have to end up as NULL rather than
                    // as an empty string.
                    ["a2", null!, "c2"],
                    ["a3"],
                ]);

            await WriteAsync(factory, path, tableData);

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                var columns = await db.Columns.AsNoTracking().OrderBy(column => column.OrdinalPosition).ToListAsync();

                Assert.Equal(["A", "B", "C"], columns.Select(column => column.Name));
                Assert.Equal([1, 2, 3], columns.Select(column => column.Id));
                Assert.Equal([1, 2, 3], columns.Select(column => column.OrdinalPosition));
            }

            var flat = await ReadCellsAsync(factory, path, 3);

            // Every row has one cell per column, so the grid can bind by index.
            Assert.Equal(
                "a1|b1|c1|a2|<null>|c2|a3|<null>|<null>",
                string.Join('|', flat.Select(value => value ?? "<null>")));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task WriteAsync_Replaces_Whatever_The_Store_Already_Held()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.tcstore");

        try
        {
            await WriteAsync(factory, path, new TableData(["Old A", "Old B"], [["1", "2"], ["3", "4"]]));
            await WriteAsync(factory, path, new TableData(["New A"], [["5"]]));

            await using var db = await factory.CreateDbContextAsync(path);

            var columns = await db.Columns.AsNoTracking().ToListAsync();

            Assert.Equal(["New A"], columns.Select(column => column.Name));

            // The old rows and cells must be gone rather than left behind for the grid to pick up.
            Assert.Equal(1, await db.Rows.AsNoTracking().CountAsync());
            Assert.Equal(1, await db.Cells.AsNoTracking().CountAsync());
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task WriteAsync_Splits_Wide_Tables_Into_Batches()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.tcstore");

        try
        {
            var headers = Enumerable.Range(0, 10).Select(index => $"Header {index}").ToArray();

            // 300 rows of 10 columns is 3000 cells, well past the 500 cell batch size, so this
            // exercises the point where a batch is flushed and the parameters are reused.
            var rows = Enumerable.Range(0, 300)
                .Select(row => headers.Select((_, column) => $"r{row}c{column}").ToArray())
                .ToArray();

            await WriteAsync(factory, path, new TableData(headers, rows));

            await using var db = await factory.CreateDbContextAsync(path);

            Assert.Equal(300, await db.Rows.AsNoTracking().CountAsync());
            Assert.Equal(3000, await db.Cells.AsNoTracking().CountAsync());

            var flat = await ReadCellsAsync(factory, path, headers.Length);

            Assert.Equal(headers.Length * rows.Length, flat.Length);
            Assert.Equal("r0c0", flat[0]);
            Assert.Equal("r299c9", flat[^1]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task WriteAsync_With_An_Empty_Table_Leaves_An_Empty_Store()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.tcstore");

        try
        {
            await WriteAsync(factory, path, new TableData([], []));

            await using var db = await factory.CreateDbContextAsync(path);

            Assert.Equal(0, await db.Columns.AsNoTracking().CountAsync());
            Assert.Equal(0, await db.Rows.AsNoTracking().CountAsync());
            Assert.Equal(0, await db.Cells.AsNoTracking().CountAsync());
        }
        finally
        {
            DeleteStore(path);
        }
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
