using Microsoft.Extensions.DependencyInjection;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.Extensions;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities.Tests.Database;

/// <summary>
/// Covers turning a table on its side. A turn moves the whole grid, the heading row included, so what is
/// asserted is the table that comes back rather than the number of statements that turned it.
/// </summary>
public class TableStoreRotationTests
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

    /// <summary>
    /// Writes a table through a sink, exactly the way an importer does: the headers become the table's
    /// columns and the rows it is given are its rows, so a table of two rows has two rows and not three.
    /// </summary>
    private static async Task WriteThroughSinkAsync(
        ITableStoreDbContextFactory factory,
        string path,
        IReadOnlyList<string> headers,
        IReadOnlyList<string?[]> rows)
    {
        await using var dbContext = await factory.CreateDbContextAsync(path);

        await using var sink = TableStoreRowSink.Create(dbContext);

        await sink.BeginAsync(headers);

        foreach (var row in rows)
        {
            await sink.WriteRowAsync(row);
        }

        await sink.CompleteAsync();
    }

    private static async Task<(IReadOnlyList<string> Headers, List<string?[]> Rows)> ReadTableAsync(
        ITableStoreDbContextFactory factory,
        string path)
    {
        await using var dbContext = await factory.CreateDbContextAsync(path);

        var source = TableStoreRowSource.Create(dbContext);

        var headers = await source.GetHeadersAsync();
        var rows = new List<string?[]>();

        await foreach (var row in source.ReadRowsAsync())
        {
            rows.Add(row);
        }

        return (headers, rows);
    }

    private static async Task<TableShape?> RotateAsync(
        ITableStoreDbContextFactory factory,
        string path,
        TableRotation rotation)
    {
        await using var dbContext = await factory.CreateDbContextAsync(path);

        return await TableStoreMaintenance.Create(dbContext).RotateAsync(rotation);
    }

    private static Task WriteTwoByTwoAsync(ITableStoreDbContextFactory factory, string path)
    {
        return WriteThroughSinkAsync(
            factory,
            path,
            ["Name", "Age"],
            [
                ["Alice", "30"],
                ["Bob", "25"],
            ]);
    }

    [Fact]
    public async Task Rotate_Clockwise_Turns_The_Whole_Grid_Heading_Row_Included()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            await WriteTwoByTwoAsync(factory, path);

            var shape = await RotateAsync(factory, path, TableRotation.Clockwise);

            var (headers, rows) = await ReadTableAsync(factory, path);

            // The left column comes to the top, read from the bottom up, and the heading row takes the
            // place at the bottom that its turn moved it to. Nothing is named afresh: every heading of the
            // result is a value the table already held.
            Assert.Equal(["Bob", "Alice", "Name"], headers);

            // The row that was at the bottom comes to the left, and it is a row of the result like any
            // other: the heading row became a column rather than being held aside.
            Assert.Single(rows);
            Assert.Equal(["25", "30", "Age"], rows[0]);

            // A grid of three rows and two columns stands on its side as two rows and three columns, and
            // the row at the top is the heading row rather than one of the rows.
            Assert.Equal(new TableShape(1, 3), shape);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Rotate_CounterClockwise_Turns_The_Grid_The_Other_Way()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            await WriteTwoByTwoAsync(factory, path);

            var shape = await RotateAsync(factory, path, TableRotation.CounterClockwise);

            var (headers, rows) = await ReadTableAsync(factory, path);

            // The right column comes to the top, read from the top down, which is the mirror of the turn
            // the other way.
            Assert.Equal(["Age", "30", "25"], headers);

            Assert.Single(rows);
            Assert.Equal(["Name", "Alice", "Bob"], rows[0]);

            Assert.Equal(new TableShape(1, 3), shape);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Rotate_One_Way_And_Then_The_Other_Leaves_The_Table_As_It_Was()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            await WriteTwoByTwoAsync(factory, path);

            await RotateAsync(factory, path, TableRotation.Clockwise);
            await RotateAsync(factory, path, TableRotation.CounterClockwise);

            var (headers, rows) = await ReadTableAsync(factory, path);

            // The two turns undo each other, down to the names of the columns.
            Assert.Equal(["Name", "Age"], headers);
            Assert.Equal(2, rows.Count);
            Assert.Equal(["Alice", "30"], rows[0]);
            Assert.Equal(["Bob", "25"], rows[1]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Rotate_The_Same_Way_Four_Times_Leaves_The_Table_As_It_Was()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            await WriteTwoByTwoAsync(factory, path);

            // Turning a table repeatedly used to grow it a column at a time, because a turn counted the
            // heading the result needed as one column more than the table it was turning.
            for (var turn = 0; turn < 4; turn++)
            {
                await RotateAsync(factory, path, TableRotation.Clockwise);
            }

            var (headers, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(["Name", "Age"], headers);
            Assert.Equal(2, rows.Count);
            Assert.Equal(["Alice", "30"], rows[0]);
            Assert.Equal(["Bob", "25"], rows[1]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Rotate_Turns_A_Table_That_Has_No_Rows_On_Its_Side()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["Name", "Age"], []);

            var shape = await RotateAsync(factory, path, TableRotation.Clockwise);

            var (headers, rows) = await ReadTableAsync(factory, path);

            // The heading row is the whole of the table, so turning it leaves a table of one column whose
            // rows are the headings that were there before.
            Assert.Equal(["Name"], headers);
            Assert.Single(rows);
            Assert.Equal(["Age"], rows[0]);
            Assert.Equal(new TableShape(1, 1), shape);

            await RotateAsync(factory, path, TableRotation.CounterClockwise);

            (headers, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(["Name", "Age"], headers);
            Assert.Empty(rows);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Rotate_Leaves_A_Table_With_No_Columns_Alone()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, [], []);

            // There is nothing to head the result with, so the table is reported as unturnable rather
            // than being emptied by a turn that cannot describe what it produced.
            Assert.Null(await RotateAsync(factory, path, TableRotation.Clockwise));

            var (headers, rows) = await ReadTableAsync(factory, path);

            Assert.Empty(headers);
            Assert.Empty(rows);
        }
        finally
        {
            DeleteStore(path);
        }
    }
}

