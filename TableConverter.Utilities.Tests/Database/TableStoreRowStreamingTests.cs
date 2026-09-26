using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.Extensions;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities.Tests.Database;

/// <summary>
/// Covers the row streaming entry points into the store, which are what let an import or an export
/// work through a table larger than the memory it runs in.
/// </summary>
public class TableStoreRowStreamingTests
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
    /// Writes a table through a sink, exactly the way an importer does.
    /// </summary>
    private static async Task WriteThroughSinkAsync(
        ITableStoreDbContextFactory factory,
        string path,
        IReadOnlyList<string> headers,
        IReadOnlyList<string?[]> rows,
        bool complete = true)
    {
        await using var dbContext = await factory.CreateDbContextAsync(path);

        await using var sink = TableStoreRowSink.Create(dbContext);

        await sink.BeginAsync(headers);

        foreach (var row in rows)
        {
            await sink.WriteRowAsync(row);
        }

        if (complete)
        {
            await sink.CompleteAsync();
        }
    }

    [Fact]
    public async Task Sink_Streams_Rows_Into_The_Store()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory,
                path,
                ["A", "B"],
                [
                    ["a1", "b1"],
                    // A short row has to be padded rather than dropped.
                    ["a2"],
                ]);

            await using var dbContext = await factory.CreateDbContextAsync(path);

            Assert.Equal(2, await dbContext.Rows.AsNoTracking().CountAsync());
            Assert.Equal(4, await dbContext.Cells.AsNoTracking().CountAsync());

            var source = TableStoreRowSource.Create(dbContext);

            Assert.Equal(["A", "B"], await source.GetHeadersAsync());

            var rows = new List<string?[]>();

            await foreach (var row in source.ReadRowsAsync())
            {
                rows.Add(row);
            }

            Assert.Equal("a1|b1", string.Join('|', rows[0]));
            Assert.Equal("a2|<null>", string.Join('|', rows[1].Select(value => value ?? "<null>")));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Sink_Handles_A_Row_Wider_Than_A_Batch()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            // 600 columns is past the 500 cell batch size, so the first batch is flushed in the middle
            // of a row. The row id has to be inserted once, and the cells that follow in the next batch
            // still have to point at it.
            var headers = Enumerable.Range(0, 600).Select(index => $"H{index}").ToArray();

            var rows = Enumerable.Range(0, 3)
                .Select(rowIndex => headers.Select((_, columnIndex) => $"r{rowIndex}c{columnIndex}").ToArray())
                .ToArray();

            await WriteThroughSinkAsync(factory, path, headers, rows);

            await using var dbContext = await factory.CreateDbContextAsync(path);

            Assert.Equal(3, await dbContext.Rows.AsNoTracking().CountAsync());
            Assert.Equal(1800, await dbContext.Cells.AsNoTracking().CountAsync());

            var source = TableStoreRowSource.Create(dbContext);

            var readBack = new List<string?[]>();

            await foreach (var row in source.ReadRowsAsync())
            {
                readBack.Add(row);
            }

            Assert.Equal(3, readBack.Count);
            Assert.Equal("r0c0", readBack[0][0]);
            Assert.Equal("r0c599", readBack[0][599]);
            Assert.Equal("r2c599", readBack[2][599]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Source_Streams_Rows_Across_Pages_In_Order()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            var rows = Enumerable.Range(0, 25)
                .Select(index => new string?[] { $"row {index}", $"{index}" })
                .ToArray();

            await WriteThroughSinkAsync(factory, path, ["Name", "Index"], rows);

            await using var dbContext = await factory.CreateDbContextAsync(path);

            // A page of four rows against twenty five rows spans several pages, including a short last
            // one, which is where an off by one in the keyset paging would show up.
            var source = TableStoreRowSource.Create(dbContext, rowsPerPage: 4);

            var readBack = new List<string[]>();

            await foreach (var row in source.ReadRowsAsync())
            {
                readBack.Add(row!);
            }

            Assert.Equal(rows.Select(row => string.Join('|', row)), readBack.Select(row => string.Join('|', row)));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Sink_Leaves_The_Store_Untouched_When_It_Is_Not_Completed()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A"], [["original"]]);

            // A parse that fails part way through disposes the sink without completing it, which has to
            // leave the table that was already there alone rather than half replaced.
            await WriteThroughSinkAsync(
                factory,
                path,
                ["Replacement A", "Replacement B"],
                [["a1", "b1"], ["a2", "b2"]],
                complete: false);

            await using var dbContext = await factory.CreateDbContextAsync(path);

            var columns = await dbContext.Columns.AsNoTracking().ToListAsync();

            Assert.Equal(["A"], columns.Select(column => column.Name));
            Assert.Equal(1, await dbContext.Rows.AsNoTracking().CountAsync());

            var cell = await dbContext.Cells.AsNoTracking().SingleAsync();

            Assert.Equal("original", cell.Value);
        }
        finally
        {
            DeleteStore(path);
        }
    }
}

