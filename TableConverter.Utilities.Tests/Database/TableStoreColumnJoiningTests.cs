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
///     Covers joining several of a table's columns into one and splitting one of them into several. What is
///     asserted is the table that comes back - where the new columns sit, the values written under them, the
///     columns that were read being taken out - and the fact that the whole of it is one step in the history
///     that can be walked both ways.
/// </summary>
public class TableStoreColumnJoiningTests
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

    private static Task WritePeopleAsync(ITableStoreDbContextFactory factory, string path)
    {
        return WriteThroughSinkAsync(
            factory,
            path,
            [new TableColumn("First", ColumnDataType.Text), new TableColumn("Last", ColumnDataType.Text)],
            [["Ada", "Lovelace"], ["Grace", "Hopper"]]);
    }

    [Fact]
    public async Task Joining_Columns_Writes_The_Result_Where_The_First_Of_Them_Sat_And_Takes_Them_Out()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WritePeopleAsync(factory, path);

            var sourceIds = await ReadColumnIdsAsync(factory, path);

            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                var added = await TableStoreColumnJoining.JoinColumnsAsync(
                    dbContext,
                    history,
                    path,
                    sourceIds,
                    "Full Name",
                    " ",
                    skipBlank: true,
                    removeSourceColumns: true,
                    "Joined columns into 'Full Name'");

                Assert.Equal("Full Name", Assert.Single(added).Name);
                Assert.Equal(1, Assert.Single(added).OrdinalPosition);
            }

            var (headers, rows) = await ReadTableAsync(factory, path);

            // The joined column takes the place the first of the columns it read held, so the table reads as
            // the one it was rather than gaining a column at the far end.
            Assert.Equal(["Full Name"], headers);
            Assert.Equal(["Ada Lovelace"], rows[0]);
            Assert.Equal(["Grace Hopper"], rows[1]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Joining_Columns_Can_Keep_The_Columns_It_Read()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WritePeopleAsync(factory, path);

            var sourceIds = await ReadColumnIdsAsync(factory, path);

            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                await TableStoreColumnJoining.JoinColumnsAsync(
                    dbContext,
                    history,
                    path,
                    sourceIds,
                    "Full Name",
                    " ",
                    skipBlank: true,
                    removeSourceColumns: false,
                    "Joined columns into 'Full Name'");
            }

            var (headers, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(["Full Name", "First", "Last"], headers);
            Assert.Equal(["Ada Lovelace", "Ada", "Lovelace"], rows[0]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Splitting_A_Column_Writes_The_Parts_Where_It_Sat_And_Takes_It_Out()
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
                [new TableColumn("Name", ColumnDataType.Text), new TableColumn("Age", ColumnDataType.Integer)],
                [["Ada Lovelace", "36"], ["Grace Hopper", "85"]]);

            var nameId = (await ReadColumnIdsAsync(factory, path))[0];

            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                var added = await TableStoreColumnJoining.SplitColumnAsync(
                    dbContext,
                    history,
                    path,
                    nameId,
                    " ",
                    ["First", "Last"],
                    removeSourceColumn: true,
                    "Split 'Name' into 'First', 'Last'");

                Assert.Equal(["First", "Last"], added.Select(column => column.Name).ToList());
            }

            var (headers, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(["First", "Last", "Age"], headers);
            Assert.Equal(["Ada", "Lovelace", "36"], rows[0]);
            Assert.Equal(["Grace", "Hopper", "85"], rows[1]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Value_With_Fewer_Parts_Leaves_The_Rest_Of_The_New_Columns_Unwritten()
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
                [new TableColumn("Name", ColumnDataType.Text)],
                [["Ada Lovelace"], ["Cher"]]);

            var nameId = (await ReadColumnIdsAsync(factory, path))[0];

            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                await TableStoreColumnJoining.SplitColumnAsync(
                    dbContext,
                    history,
                    path,
                    nameId,
                    " ",
                    ["First", "Last"],
                    removeSourceColumn: true,
                    "Split 'Name'");
            }

            var (_, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(["Ada", "Lovelace"], rows[0]);

            // A value with only one part has nothing to put in the second column, so the cell reads as
            // nothing rather than as an empty string.
            Assert.Equal(["Cher", null], rows[1]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Joining_Columns_Is_One_Step_In_The_History()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WritePeopleAsync(factory, path);

            var sourceIds = await ReadColumnIdsAsync(factory, path);

            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                await TableStoreColumnJoining.JoinColumnsAsync(
                    dbContext,
                    history,
                    path,
                    sourceIds,
                    "Full Name",
                    " ",
                    skipBlank: true,
                    removeSourceColumns: true,
                    "Joined columns into 'Full Name'");
            }

            Assert.Equal(1, await history.GetUndoDepthAsync(path));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Taking_The_Step_Back_Puts_The_Columns_And_Their_Values_Back()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WritePeopleAsync(factory, path);

            var sourceIds = await ReadColumnIdsAsync(factory, path);

            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                await TableStoreColumnJoining.JoinColumnsAsync(
                    dbContext,
                    history,
                    path,
                    sourceIds,
                    "Full Name",
                    " ",
                    skipBlank: true,
                    removeSourceColumns: true,
                    "Joined columns into 'Full Name'");
            }

            await history.UndoAsync(path);

            var (headers, rows) = await ReadTableAsync(factory, path);

            // Taking the step back puts back both the columns that were read and the values under them.
            Assert.Equal(["First", "Last"], headers);
            Assert.Equal(["Ada", "Lovelace"], rows[0]);
            Assert.Equal(["Grace", "Hopper"], rows[1]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Putting_The_Step_In_Place_Again_Brings_The_Joined_Values_With_It()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WritePeopleAsync(factory, path);

            var sourceIds = await ReadColumnIdsAsync(factory, path);

            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                await TableStoreColumnJoining.JoinColumnsAsync(
                    dbContext,
                    history,
                    path,
                    sourceIds,
                    "Full Name",
                    " ",
                    skipBlank: true,
                    removeSourceColumns: true,
                    "Joined columns into 'Full Name'");
            }

            await history.UndoAsync(path);
            await history.RedoAsync(path);

            var (headers, rows) = await ReadTableAsync(factory, path);

            // The values the join worked out are remembered with the step, so putting it in place again
            // brings them back rather than leaving the new column empty.
            Assert.Equal(["Full Name"], headers);
            Assert.Equal(["Ada Lovelace"], rows[0]);
            Assert.Equal(["Grace Hopper"], rows[1]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Blank_Value_Is_Left_Out_Of_A_Join_When_It_Is_Asked_To_Be()
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
                [new TableColumn("First", ColumnDataType.Text), new TableColumn("Middle", ColumnDataType.Text)],
                [["Ada", "Augusta"], ["Grace", null]]);

            var sourceIds = await ReadColumnIdsAsync(factory, path);

            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                await TableStoreColumnJoining.JoinColumnsAsync(
                    dbContext,
                    history,
                    path,
                    sourceIds,
                    "Full Name",
                    " ",
                    skipBlank: true,
                    removeSourceColumns: true,
                    "Joined columns into 'Full Name'");
            }

            var (_, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(["Ada Augusta"], rows[0]);

            // The blank value is left out of the join rather than joined as an empty piece, so the value is
            // just the one that was there.
            Assert.Equal(["Grace"], rows[1]);
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
                await TableStoreColumnJoining.JoinColumnsAsync(
                    dbContext,
                    history,
                    path,
                    sourceIds,
                    "Full Name",
                    " ",
                    skipBlank: true,
                    removeSourceColumns: true,
                    "Joined columns into 'Full Name'",
                    batchSize: 2);
            }

            var (_, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(5, rows.Count);
            Assert.Equal(["A 1"], rows[0]);
            Assert.Equal(["C 3"], rows[2]);
            Assert.Equal(["E 5"], rows[4]);
        }
        finally
        {
            DeleteStore(path);
        }
    }
}

