using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.Extensions;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities.Tests.Database;

/// <summary>
/// Covers <see cref="TableStoreDataReader" />, which is what reads a store back out so it can be
/// exported.
/// </summary>
public class TableStoreDataReaderTests
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddSingleton<IEventManager, EventManager>();
        services.AddTableStoreDatabase();

        return services.BuildServiceProvider();
    }

    private static string NewStorePath()
    {
        return Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.tcstore");
    }

    private static async Task<TableData> ReadAsync(ITableStoreDbContextFactory factory, string path)
    {
        await using var dbContext = await factory.CreateDbContextAsync(path);

        return await TableStoreDataReader.ReadAsync(dbContext);
    }

    /// <summary>
    /// Renders a row as text so an assertion can tell a missing value apart from an empty one.
    /// </summary>
    private static string Describe(IEnumerable<string?> row)
    {
        return string.Join('|', row.Select(value => value ?? "<null>"));
    }

    [Fact]
    public async Task ReadAsync_Returns_What_The_Writer_Stored()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            var written = new TableData(
                ["A", "B", "C"],
                [
                    ["a1", "b1", "c1"],
                    ["a2", null!, "c2"],
                    ["a3"],
                ]);

            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                await TableStoreDataWriter.WriteAsync(dbContext, written);
            }

            var read = await ReadAsync(factory, path);

            Assert.Equal(["A", "B", "C"], read.Headers);

            Assert.Equal(3, read.Rows.Count);

            // A short row is padded, and a missing value stays missing rather than becoming an empty
            // string, so an export writes the same table that was imported.
            Assert.Equal(Describe(["a1", "b1", "c1"]), Describe(read.Rows[0]));
            Assert.Equal(Describe(["a2", null, "c2"]), Describe(read.Rows[1]));
            Assert.Equal(Describe(["a3", null, null]), Describe(read.Rows[2]));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task ReadAsync_Keeps_Rows_In_The_Order_They_Were_Added()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            var rows = Enumerable.Range(0, 50)
                .Select(index => new[] { $"row {index}", $"{index}" })
                .ToArray();

            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                await TableStoreDataWriter.WriteAsync(dbContext, new TableData(["Name", "Index"], rows));
            }

            var read = await ReadAsync(factory, path);

            Assert.Equal(rows, read.Rows);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task ReadAsync_Positions_Values_By_Column_Ordinal_Not_By_Column_Id()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            // Deleting a column leaves a gap in the ids, so the id of a column is not its index. A
            // reader that indexed by id would put these values in the wrong columns.
            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                dbContext.Columns.Add(new ColumnEntity { Id = 7, Name = "Second", OrdinalPosition = 2 });
                dbContext.Columns.Add(new ColumnEntity { Id = 1, Name = "First", OrdinalPosition = 1 });
                dbContext.Columns.Add(new ColumnEntity { Id = 9, Name = "Third", OrdinalPosition = 3 });

                var complete = new RowEntity();
                var partial = new RowEntity();

                dbContext.Rows.Add(complete);
                dbContext.Rows.Add(partial);

                await dbContext.SaveChangesAsync();

                dbContext.Cells.Add(new CellEntity { RowId = complete.Id, ColumnId = 7, Value = "b" });
                dbContext.Cells.Add(new CellEntity { RowId = complete.Id, ColumnId = 9, Value = "c" });
                dbContext.Cells.Add(new CellEntity { RowId = complete.Id, ColumnId = 1, Value = "a" });

                // Only the last column is filled in, so the other two have to come back as null rather
                // than being shifted up.
                dbContext.Cells.Add(new CellEntity { RowId = partial.Id, ColumnId = 9, Value = "c2" });

                await dbContext.SaveChangesAsync();
            }

            var read = await ReadAsync(factory, path);

            Assert.Equal(["First", "Second", "Third"], read.Headers);
            Assert.Equal(Describe(["a", "b", "c"]), Describe(read.Rows[0]));
            Assert.Equal(Describe([null, null, "c2"]), Describe(read.Rows[1]));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task ReadAsync_With_An_Empty_Store_Returns_An_Empty_Table()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            var read = await ReadAsync(factory, path);

            Assert.Empty(read.Headers);
            Assert.Empty(read.Rows);
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

