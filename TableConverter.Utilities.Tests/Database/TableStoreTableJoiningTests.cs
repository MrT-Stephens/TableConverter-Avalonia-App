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
///     Covers bringing a second table into a table: merging the rows of another table in, and joining the
///     columns of another table onto the rows that match it on a key. What is asserted is the table that
///     comes back - the rows that were added, the columns that were brought across, the values that landed
///     under them - and the fact that the whole of it is one step in the history that can be walked both
///     ways.
/// </summary>
public class TableStoreTableJoiningTests
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

    private static async Task<TResult> WithSourceAsync<TResult>(
        ITableStoreDbContextFactory factory,
        string sourcePath,
        Func<ITableRowSource, Task<TResult>> work)
    {
        await using var dbContext = await factory.CreateDbContextAsync(sourcePath);

        return await work(TableStoreRowSource.Create(dbContext));
    }

    [Fact]
    public async Task Merging_A_Table_Adds_Its_Rows_After_The_Ones_Already_Held()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();
        var sourcePath = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory, path,
                [new TableColumn("Name", ColumnDataType.Text)],
                [["Ada"], ["Grace"]]);

            await WriteThroughSinkAsync(
                factory, sourcePath,
                [new TableColumn("Name", ColumnDataType.Text)],
                [["Alan"], ["Edsger"]]);

            var result = await WithSourceAsync(factory, sourcePath, async source =>
            {
                await using var dbContext = await factory.CreateDbContextAsync(path);

                return await TableStoreTableJoining.AppendAsync(dbContext, history, path, source, "Merged a table in");
            });

            var (headers, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(2, result.AppendedRowCount);
            Assert.Empty(result.AddedColumns);
            Assert.Equal(["Name"], headers);
            Assert.Equal(["Ada"], rows[0]);
            Assert.Equal(["Grace"], rows[1]);
            Assert.Equal(["Alan"], rows[2]);
            Assert.Equal(["Edsger"], rows[3]);
        }
        finally
        {
            DeleteStore(path);
            DeleteStore(sourcePath);
        }
    }

    [Fact]
    public async Task Merging_Adds_A_Column_The_Table_Does_Not_Hold_And_Leaves_It_Empty_For_The_Rows_Already_There()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();
        var sourcePath = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory, path,
                [new TableColumn("Name", ColumnDataType.Text)],
                [["Ada"]]);

            await WriteThroughSinkAsync(
                factory, sourcePath,
                [new TableColumn("Name", ColumnDataType.Text), new TableColumn("Age", ColumnDataType.Integer)],
                [["Alan", "41"]]);

            var result = await WithSourceAsync(factory, sourcePath, async source =>
            {
                await using var dbContext = await factory.CreateDbContextAsync(path);

                return await TableStoreTableJoining.AppendAsync(dbContext, history, path, source, "Merged a table in");
            });

            var (headers, rows) = await ReadTableAsync(factory, path);

            Assert.Equal("Age", Assert.Single(result.AddedColumns).Name);
            Assert.Equal(["Name", "Age"], headers);

            // The row that was already there holds nothing under the column the other table brought, which is
            // how a value that is not there reads everywhere else.
            Assert.Equal(["Ada", null], rows[0]);
            Assert.Equal(["Alan", "41"], rows[1]);
        }
        finally
        {
            DeleteStore(path);
            DeleteStore(sourcePath);
        }
    }

    [Fact]
    public async Task Merging_Matches_Columns_Up_By_Name_Not_By_Where_They_Sit()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();
        var sourcePath = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory, path,
                [new TableColumn("Name", ColumnDataType.Text), new TableColumn("Age", ColumnDataType.Integer)],
                [["Ada", "36"]]);

            // The other table holds the same two columns the other way round, so a value only lands where it
            // belongs if the columns are matched by name rather than by the place they sit in.
            await WriteThroughSinkAsync(
                factory, sourcePath,
                [new TableColumn("Age", ColumnDataType.Integer), new TableColumn("Name", ColumnDataType.Text)],
                [["41", "Alan"]]);

            await WithSourceAsync(factory, sourcePath, async source =>
            {
                await using var dbContext = await factory.CreateDbContextAsync(path);

                return await TableStoreTableJoining.AppendAsync(dbContext, history, path, source, "Merged a table in");
            });

            var (headers, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(["Name", "Age"], headers);
            Assert.Equal(["Ada", "36"], rows[0]);
            Assert.Equal(["Alan", "41"], rows[1]);
        }
        finally
        {
            DeleteStore(path);
            DeleteStore(sourcePath);
        }
    }

    [Fact]
    public async Task Merging_Is_One_Step_In_The_History_And_Undoing_Takes_The_Rows_And_Columns_With_It()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();
        var sourcePath = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory, path,
                [new TableColumn("Name", ColumnDataType.Text)],
                [["Ada"]]);

            await WriteThroughSinkAsync(
                factory, sourcePath,
                [new TableColumn("Name", ColumnDataType.Text), new TableColumn("Age", ColumnDataType.Integer)],
                [["Alan", "41"]]);

            await WithSourceAsync(factory, sourcePath, async source =>
            {
                await using var dbContext = await factory.CreateDbContextAsync(path);

                return await TableStoreTableJoining.AppendAsync(dbContext, history, path, source, "Merged a table in");
            });

            Assert.Equal(1, await history.GetUndoDepthAsync(path));

            await history.UndoAsync(path);

            var (headers, rows) = await ReadTableAsync(factory, path);

            // Taking the step back takes the rows that were merged in and the column the other table brought,
            // leaving the table exactly as it was.
            Assert.Equal(["Name"], headers);
            Assert.Equal(["Ada"], Assert.Single(rows));

            await history.RedoAsync(path);

            var (redoneHeaders, redoneRows) = await ReadTableAsync(factory, path);

            // Putting the step in place again brings back both the rows and the column they were written under.
            Assert.Equal(["Name", "Age"], redoneHeaders);
            Assert.Equal(["Ada", null], redoneRows[0]);
            Assert.Equal(["Alan", "41"], redoneRows[1]);
        }
        finally
        {
            DeleteStore(path);
            DeleteStore(sourcePath);
        }
    }

    [Fact]
    public async Task Every_Row_Is_Merged_Even_When_The_Work_Is_Done_A_Batch_At_A_Time()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();
        var sourcePath = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory, path,
                [new TableColumn("Name", ColumnDataType.Text)],
                [["Ada"]]);

            await WriteThroughSinkAsync(
                factory, sourcePath,
                [new TableColumn("Name", ColumnDataType.Text)],
                [["A"], ["B"], ["C"], ["D"], ["E"]]);

            var result = await WithSourceAsync(factory, sourcePath, async source =>
            {
                await using var dbContext = await factory.CreateDbContextAsync(path);

                return await TableStoreTableJoining.AppendAsync(
                    dbContext, history, path, source, "Merged a table in", batchSize: 2);
            });

            var (_, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(5, result.AppendedRowCount);
            Assert.Equal(6, rows.Count);
            Assert.Equal(["E"], rows[5]);

            // Every batch is remembered with the step, so taking it back takes the whole merge with it.
            await history.UndoAsync(path);

            var (_, undone) = await ReadTableAsync(factory, path);

            Assert.Equal(["Ada"], Assert.Single(undone));
        }
        finally
        {
            DeleteStore(path);
            DeleteStore(sourcePath);
        }
    }

    [Fact]
    public async Task Joining_A_Table_Brings_Its_Columns_Over_For_The_Rows_That_Match()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();
        var sourcePath = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory, path,
                [new TableColumn("Id", ColumnDataType.Integer), new TableColumn("Name", ColumnDataType.Text)],
                [["1", "Ada"], ["2", "Grace"]]);

            await WriteThroughSinkAsync(
                factory, sourcePath,
                [new TableColumn("Id", ColumnDataType.Integer), new TableColumn("City", ColumnDataType.Text)],
                [["1", "London"], ["2", "New York"]]);

            var result = await WithSourceAsync(factory, sourcePath, async source =>
            {
                await using var dbContext = await factory.CreateDbContextAsync(path);

                return await TableStoreTableJoining.JoinAsync(
                    dbContext, history, path, source, [new ColumnMatch("Id", "Id")], "Joined a table on");
            });

            var (headers, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(2, result.MatchedRowCount);
            Assert.Equal("City", Assert.Single(result.AddedColumns).Name);

            // The key column is the join itself, so it is only there once.
            Assert.Equal(["Id", "Name", "City"], headers);
            Assert.Equal(["1", "Ada", "London"], rows[0]);
            Assert.Equal(["2", "Grace", "New York"], rows[1]);
        }
        finally
        {
            DeleteStore(path);
            DeleteStore(sourcePath);
        }
    }

    [Fact]
    public async Task A_Row_That_Matches_Nothing_Keeps_Nothing_Under_The_New_Columns()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();
        var sourcePath = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory, path,
                [new TableColumn("Id", ColumnDataType.Integer), new TableColumn("Name", ColumnDataType.Text)],
                [["1", "Ada"], ["9", "Nobody"]]);

            await WriteThroughSinkAsync(
                factory, sourcePath,
                [new TableColumn("Id", ColumnDataType.Integer), new TableColumn("City", ColumnDataType.Text)],
                [["1", "London"]]);

            var result = await WithSourceAsync(factory, sourcePath, async source =>
            {
                await using var dbContext = await factory.CreateDbContextAsync(path);

                return await TableStoreTableJoining.JoinAsync(
                    dbContext, history, path, source, [new ColumnMatch("Id", "Id")], "Joined a table on");
            });

            var (_, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(1, result.MatchedRowCount);
            Assert.Equal(["9", "Nobody", null], rows[1]);
        }
        finally
        {
            DeleteStore(path);
            DeleteStore(sourcePath);
        }
    }

    [Fact]
    public async Task The_First_Row_That_Holds_A_Key_Is_The_One_That_Matches()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();
        var sourcePath = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory, path,
                [new TableColumn("Id", ColumnDataType.Integer), new TableColumn("Name", ColumnDataType.Text)],
                [["1", "Ada"]]);

            // The other table holds the same key twice, so which one matches has to be decided rather than
            // left to chance: the one it meets first is the one that wins.
            await WriteThroughSinkAsync(
                factory, sourcePath,
                [new TableColumn("Id", ColumnDataType.Integer), new TableColumn("City", ColumnDataType.Text)],
                [["1", "London"], ["1", "Paris"]]);

            await WithSourceAsync(factory, sourcePath, async source =>
            {
                await using var dbContext = await factory.CreateDbContextAsync(path);

                return await TableStoreTableJoining.JoinAsync(
                    dbContext, history, path, source, [new ColumnMatch("Id", "Id")], "Joined a table on");
            });

            var (_, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(["1", "Ada", "London"], rows[0]);
        }
        finally
        {
            DeleteStore(path);
            DeleteStore(sourcePath);
        }
    }

    [Fact]
    public async Task Joining_Is_One_Step_In_The_History_And_Taking_It_Back_Takes_The_Columns_And_Values_With_It()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();
        var sourcePath = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory, path,
                [new TableColumn("Id", ColumnDataType.Integer), new TableColumn("Name", ColumnDataType.Text)],
                [["1", "Ada"], ["2", "Grace"]]);

            await WriteThroughSinkAsync(
                factory, sourcePath,
                [new TableColumn("Id", ColumnDataType.Integer), new TableColumn("City", ColumnDataType.Text)],
                [["1", "London"], ["2", "New York"]]);

            await WithSourceAsync(factory, sourcePath, async source =>
            {
                await using var dbContext = await factory.CreateDbContextAsync(path);

                return await TableStoreTableJoining.JoinAsync(
                    dbContext, history, path, source, [new ColumnMatch("Id", "Id")], "Joined a table on");
            });

            Assert.Equal(1, await history.GetUndoDepthAsync(path));

            await history.UndoAsync(path);

            var (headers, rows) = await ReadTableAsync(factory, path);

            // Taking the step back takes the column the other table brought, and the values under it, leaving
            // the table exactly as it was.
            Assert.Equal(["Id", "Name"], headers);
            Assert.Equal(["1", "Ada"], rows[0]);

            await history.RedoAsync(path);

            var (redoneHeaders, redoneRows) = await ReadTableAsync(factory, path);

            // Putting the step in place again brings the column back with the values that were joined under it.
            Assert.Equal(["Id", "Name", "City"], redoneHeaders);
            Assert.Equal(["1", "Ada", "London"], redoneRows[0]);
            Assert.Equal(["2", "Grace", "New York"], redoneRows[1]);
        }
        finally
        {
            DeleteStore(path);
            DeleteStore(sourcePath);
        }
    }

    [Fact]
    public async Task Every_Row_Is_Joined_Even_When_The_Work_Is_Done_A_Batch_At_A_Time()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();
        var sourcePath = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory, path,
                [new TableColumn("Id", ColumnDataType.Integer), new TableColumn("Name", ColumnDataType.Text)],
                [["1", "A"], ["2", "B"], ["3", "C"], ["4", "D"], ["5", "E"]]);

            await WriteThroughSinkAsync(
                factory, sourcePath,
                [new TableColumn("Id", ColumnDataType.Integer), new TableColumn("City", ColumnDataType.Text)],
                [["1", "One"], ["2", "Two"], ["3", "Three"], ["4", "Four"], ["5", "Five"]]);

            await WithSourceAsync(factory, sourcePath, async source =>
            {
                await using var dbContext = await factory.CreateDbContextAsync(path);

                return await TableStoreTableJoining.JoinAsync(
                    dbContext, history, path, source, [new ColumnMatch("Id", "Id")], "Joined a table on",
                    batchSize: 2);
            });

            var (_, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(["1", "A", "One"], rows[0]);
            Assert.Equal(["3", "C", "Three"], rows[2]);
            Assert.Equal(["5", "E", "Five"], rows[4]);
        }
        finally
        {
            DeleteStore(path);
            DeleteStore(sourcePath);
        }
    }

    [Fact]
    public async Task Joining_Two_Columns_Matches_On_The_Combination_Of_Them()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();
        var sourcePath = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory, path,
                [
                    new TableColumn("First", ColumnDataType.Text),
                    new TableColumn("Last", ColumnDataType.Text),
                    new TableColumn("Role", ColumnDataType.Text),
                ],
                [["Ada", "Lovelace", "Mathematician"], ["Ada", "Byron", "Writer"]]);

            await WriteThroughSinkAsync(
                factory, sourcePath,
                [
                    new TableColumn("Given", ColumnDataType.Text),
                    new TableColumn("Family", ColumnDataType.Text),
                    new TableColumn("Born", ColumnDataType.Integer),
                ],
                [["Ada", "Lovelace", "1815"], ["Ada", "Byron", "1780"]]);

            await WithSourceAsync(factory, sourcePath, async source =>
            {
                await using var dbContext = await factory.CreateDbContextAsync(path);

                return await TableStoreTableJoining.JoinAsync(
                    dbContext, history, path, source,
                    [new ColumnMatch("First", "Given"), new ColumnMatch("Last", "Family")],
                    "Joined a table on");
            });

            var (headers, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(["First", "Last", "Role", "Born"], headers);
            Assert.Equal(["Ada", "Lovelace", "Mathematician", "1815"], rows[0]);
            Assert.Equal(["Ada", "Byron", "Writer", "1780"], rows[1]);
        }
        finally
        {
            DeleteStore(path);
            DeleteStore(sourcePath);
        }
    }

    [Fact]
    public async Task A_Row_That_Is_Missing_Part_Of_Its_Key_Matches_Nothing()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();
        var sourcePath = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory, path,
                [new TableColumn("First", ColumnDataType.Text), new TableColumn("Last", ColumnDataType.Text)],
                [["Ada", null], ["Ada", "Lovelace"]]);

            await WriteThroughSinkAsync(
                factory, sourcePath,
                [new TableColumn("First", ColumnDataType.Text), new TableColumn("Last", ColumnDataType.Text),
                    new TableColumn("Role", ColumnDataType.Text)],
                [["Ada", "Lovelace", "Mathematician"]]);

            var result = await WithSourceAsync(factory, sourcePath, async source =>
            {
                await using var dbContext = await factory.CreateDbContextAsync(path);

                return await TableStoreTableJoining.JoinAsync(
                    dbContext, history, path, source,
                    [new ColumnMatch("First", "First"), new ColumnMatch("Last", "Last")],
                    "Joined a table on");
            });

            var (_, rows) = await ReadTableAsync(factory, path);

            // A row that is missing part of its key cannot be said to match another, so only the row that
            // holds the whole of its key finds one.
            Assert.Equal(1, result.MatchedRowCount);
            Assert.Equal(["Ada", null, null], rows[0]);
            Assert.Equal(["Ada", "Lovelace", "Mathematician"], rows[1]);

        }
        finally
        {
            DeleteStore(path);
            DeleteStore(sourcePath);
        }
    }

    [Fact]
    public async Task Asking_For_A_Column_The_Table_Has_Not_Got_Is_Refused()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();
        var sourcePath = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory, path,
                [new TableColumn("Id", ColumnDataType.Integer)],
                [["1"]]);

            await WriteThroughSinkAsync(
                factory, sourcePath,
                [new TableColumn("Id", ColumnDataType.Integer), new TableColumn("City", ColumnDataType.Text)],
                [["1", "London"]]);

            await Assert.ThrowsAsync<ArgumentException>(() => WithSourceAsync(factory, sourcePath, async source =>
            {
                await using var dbContext = await factory.CreateDbContextAsync(path);

                return await TableStoreTableJoining.JoinAsync(
                    dbContext, history, path, source, [new ColumnMatch("Name", "Id")], "Joined a table on");
            }));
        }
        finally
        {
            DeleteStore(path);
            DeleteStore(sourcePath);
        }
    }
}

