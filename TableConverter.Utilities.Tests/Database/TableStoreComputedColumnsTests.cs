using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.Extensions;
using TableConverter.Utilities.Database.History;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Interfaces;
using TableConverter.Utilities.Models;

namespace TableConverter.Utilities.Tests.Database;

/// <summary>
///     Covers adding columns whose values are worked out from the values a row already holds. What is
///     asserted is the table that comes back - the columns that were added, the values written under them,
///     and the fact that the whole of it is one step in the history.
/// </summary>
public class TableStoreComputedColumnsTests
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

    private static async Task WriteThroughSinkAsync(
        ITableStoreDbContextFactory factory,
        string path,
        IReadOnlyList<TableColumn> columns,
        IReadOnlyList<string?[]> rows)
    {
        await using var dbContext = await factory.CreateDbContextAsync(path);

        await using var sink = TableStoreRowSink.Create(dbContext);

        await sink.BeginAsync(columns);

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

    private static async Task<IReadOnlyList<string>> ReadHeadersAsync(
        ITableStoreDbContextFactory factory,
        string path)
    {
        var (headers, _) = await ReadTableAsync(factory, path);

        return headers;
    }

    private static async Task<IReadOnlyList<int>> ReadColumnIdsAsync(
        ITableStoreDbContextFactory factory,
        string path)
    {
        await using var dbContext = await factory.CreateDbContextAsync(path);

        return await dbContext.Columns
            .OrderBy(column => column.OrdinalPosition)
            .Select(column => column.Id)
            .ToListAsync();
    }

    private static ComputedColumnDefinition Join(string name, string separator, bool skipBlank)
    {
        return new ComputedColumnDefinition(
            name,
            ColumnDataType.Text,
            values => ComputedValueOperations.Concatenate(values, separator, skipBlank));
    }

    [Fact]
    public async Task A_Derived_Column_Is_Added_And_Filled_From_The_Values_Of_Its_Row()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory,
                path,
                [new TableColumn("First", ColumnDataType.Text), new TableColumn("Last", ColumnDataType.Text)],
                [["Ada", "Lovelace"], ["Grace", "Hopper"]]);

            var sourceIds = await ReadColumnIdsAsync(factory, path);

            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                var added = await TableStoreComputedColumns.AddAsync(
                    dbContext,
                    history,
                    path,
                    sourceIds,
                    [Join("Full Name", " ", skipBlank: true)],
                    "Added derived column 'Full Name'");

                Assert.Equal("Full Name", Assert.Single(added).Name);
            }

            var (headers, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(["First", "Last", "Full Name"], headers);
            Assert.Equal(["Ada", "Lovelace", "Ada Lovelace"], rows[0]);
            Assert.Equal(["Grace", "Hopper", "Grace Hopper"], rows[1]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Deriving_A_Column_Is_One_Step_In_The_History()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory,
                path,
                [new TableColumn("First", ColumnDataType.Text), new TableColumn("Last", ColumnDataType.Text)],
                [["Ada", "Lovelace"], ["Grace", "Hopper"]]);

            var sourceIds = await ReadColumnIdsAsync(factory, path);

            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                await TableStoreComputedColumns.AddAsync(
                    dbContext,
                    history,
                    path,
                    sourceIds,
                    [Join("Full Name", " ", skipBlank: true)],
                    "Added derived column 'Full Name'");
            }

            Assert.Equal(1, await history.GetUndoDepthAsync(path));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Taking_The_Step_Back_Takes_The_Derived_Column_And_Its_Values_With_It()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory,
                path,
                [new TableColumn("First", ColumnDataType.Text), new TableColumn("Last", ColumnDataType.Text)],
                [["Ada", "Lovelace"], ["Grace", "Hopper"]]);

            var sourceIds = await ReadColumnIdsAsync(factory, path);

            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                await TableStoreComputedColumns.AddAsync(
                    dbContext,
                    history,
                    path,
                    sourceIds,
                    [Join("Full Name", " ", skipBlank: true)],
                    "Added derived column 'Full Name'");
            }

            await history.UndoAsync(path);

            var headers = await ReadHeadersAsync(factory, path);

            Assert.Equal(["First", "Last"], headers);

            var (_, rows) = await ReadTableAsync(factory, path);

            // Taking the column back takes the column itself with it, so a row is its original width again
            // rather than keeping a gap where the derived column used to sit.
            Assert.Equal(["Ada", "Lovelace"], rows[0]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Row_Without_One_Of_The_Source_Values_Leaves_The_Cell_Unwritten()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory,
                path,
                [new TableColumn("First", ColumnDataType.Text), new TableColumn("Last", ColumnDataType.Text)],
                [["Ada", "Lovelace"], ["Cher", null]]);

            var sourceIds = await ReadColumnIdsAsync(factory, path);

            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                await TableStoreComputedColumns.AddAsync(
                    dbContext,
                    history,
                    path,
                    sourceIds,
                    [Join("Full Name", " ", skipBlank: true)],
                    "Added derived column 'Full Name'");
            }

            var (_, rows) = await ReadTableAsync(factory, path);

            Assert.Equal("Ada Lovelace", rows[0][2]);

            // Only one value is there to be joined, so the derived value is just that one rather than a
            // value with a gap in it.
            Assert.Equal("Cher", rows[1][2]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task More_Than_One_Column_Is_Derived_From_The_Same_Row()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory,
                path,
                [new TableColumn("First", ColumnDataType.Text), new TableColumn("Last", ColumnDataType.Text)],
                [["Ada", "Lovelace"]]);

            var sourceIds = await ReadColumnIdsAsync(factory, path);

            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                await TableStoreComputedColumns.AddAsync(
                    dbContext,
                    history,
                    path,
                    sourceIds,
                    [
                        Join("Full Name", " ", skipBlank: true),
                        Join("Initials", ". ", skipBlank: true),
                    ],
                    "Added 2 derived columns");
            }

            var (headers, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(["First", "Last", "Full Name", "Initials"], headers);
            Assert.Equal(["Ada", "Lovelace", "Ada Lovelace", "Ada. Lovelace"], rows[0]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Every_Row_Is_Filled_Even_When_The_Work_Is_Done_A_Batch_At_A_Time()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory,
                path,
                [new TableColumn("First", ColumnDataType.Text), new TableColumn("Last", ColumnDataType.Text)],
                [
                    ["A", "1"],
                    ["B", "2"],
                    ["C", "3"],
                    ["D", "4"],
                    ["E", "5"],
                ]);

            var sourceIds = await ReadColumnIdsAsync(factory, path);

            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                await TableStoreComputedColumns.AddAsync(
                    dbContext,
                    history,
                    path,
                    sourceIds,
                    [Join("Full Name", " ", skipBlank: true)],
                    "Added derived column 'Full Name'",
                    batchSize: 2);
            }

            var (_, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(5, rows.Count);
            Assert.Equal("A 1", rows[0][2]);
            Assert.Equal("C 3", rows[2][2]);
            Assert.Equal("E 5", rows[4][2]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Deriving_Nothing_Adds_Nothing_And_Records_Nothing()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory,
                path,
                [new TableColumn("First", ColumnDataType.Text)],
                [["Ada"]]);

            var sourceIds = await ReadColumnIdsAsync(factory, path);

            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                var added = await TableStoreComputedColumns.AddAsync(
                    dbContext,
                    history,
                    path,
                    sourceIds,
                    [],
                    "Added 0 derived columns");

                Assert.Empty(added);
            }

            Assert.Equal(0, await history.GetUndoDepthAsync(path));

            var headers = await ReadHeadersAsync(factory, path);

            Assert.Equal(["First"], headers);
        }
        finally
        {
            DeleteStore(path);
        }
    }
}