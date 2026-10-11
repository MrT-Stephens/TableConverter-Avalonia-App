using System.Collections.Specialized;
using Microsoft.Extensions.DependencyInjection;
using ModelFlow.DataVirtualization;
using TableConverter.Services.DataSources;
using TableConverter.Utilities;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.Extensions;
using TableConverter.Utilities.Database.History;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Utilities.Interfaces;
using TableConverter.Utilities.Models;

namespace TableConverter.Tests.Columns;

/// <summary>
///     Exercises the columns editor's own operations - moving a column along the table, copying one, and
///     the edits the store refuses - so what the tool offers a user is what is checked.
/// </summary>
public class ColumnManagementTests
{
    /// <summary>
    ///     Runs the actions ModelFlow would hand to the UI thread where they are raised. A data source cannot
    ///     be built without a hook, and a test host has no dispatcher to hand them to.
    /// </summary>
    /// <remarks>
    ///     The hook is one value shared by the whole process, so it is put back the way it was found rather
    ///     than left set for whatever runs next.
    /// </remarks>
    private sealed class InlineUiThread : IDisposable
    {
        private readonly Func<Action, Task>? _previous = VirtualizationManager.Instance.UiThreadExcecuteAction;

        public InlineUiThread()
        {
            VirtualizationManager.Instance.UiThreadExcecuteAction = action =>
            {
                action();
                return Task.CompletedTask;
            };
        }

        public void Dispose()
        {
            VirtualizationManager.Instance.UiThreadExcecuteAction = _previous;
        }
    }

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

    private static IReadOnlyList<TableColumn> ThreeTextColumns()
    {
        return
        [
            new TableColumn("A", ColumnDataType.Text),
            new TableColumn("B", ColumnDataType.Text),
            new TableColumn("C", ColumnDataType.Text),
        ];
    }

    private static Task<ColumnEntity?> FindColumnAsync(TableStoreColumnsDataSource source, string name)
    {
        return source.GetItemAsync(column => column.Name == name);
    }

    /// <summary>
    ///     Compares a row of values against what it is expected to hold, value by value, so a row that has
    ///     been read as a sequence of nullable texts is compared without the collection expression having to
    ///     say what its own values may be.
    /// </summary>
    private static void AssertRow(string?[] expected, string?[] actual)
    {
        Assert.Equal(expected.Length, actual.Length);

        for (var index = 0; index < expected.Length; index++)
        {
            Assert.Equal(expected[index], actual[index]);
        }
    }

    [Fact]
    public async Task Moving_A_Column_Right_Exchanges_It_With_The_Column_Beside_It()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            using var uiThread = new InlineUiThread();

            await WriteThroughSinkAsync(factory, path, ThreeTextColumns(), [["a1", "b1", "c1"], ["a2", "b2", "c2"]]);

            var source = new TableStoreColumnsDataSource(factory, history) { Path = path };

            var columnB = await FindColumnAsync(source, "B");

            Assert.NotNull(columnB);

            Assert.True(await source.MoveAsync(columnB, 1));

            // The column keeps what it holds and takes the place of the column that followed it: the values
            // under a column are read by the column's own id, so moving the column moves its values with it.
            var (movedHeaders, movedRows) = await ReadTableAsync(factory, path);

            Assert.Equal(["A", "C", "B"], movedHeaders);
            AssertRow(["a1", "c1", "b1"], movedRows[0]);
            AssertRow(["a2", "c2", "b2"], movedRows[1]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Column_Moved_Right_Is_Taken_Back_To_The_Place_It_Held()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            using var uiThread = new InlineUiThread();

            await WriteThroughSinkAsync(factory, path, ThreeTextColumns(), [["a1", "b1", "c1"]]);

            var source = new TableStoreColumnsDataSource(factory, history) { Path = path };

            var columnB = await FindColumnAsync(source, "B");

            Assert.NotNull(columnB);

            Assert.True(await source.MoveAsync(columnB, 1));

            var undone = await history.UndoAsync(path);

            Assert.NotNull(undone);
            Assert.Equal("Moved a column", undone.Description);

            // Taking the move back puts the columns in the places they held and leaves the values under them
            // where they were, because a move changes only what a column is placed by.
            var (undoneHeaders, undoneRows) = await ReadTableAsync(factory, path);

            Assert.Equal(["A", "B", "C"], undoneHeaders);
            AssertRow(["a1", "b1", "c1"], undoneRows[0]);

            await history.RedoAsync(path);

            var (redoneHeaders, _) = await ReadTableAsync(factory, path);

            Assert.Equal(["A", "C", "B"], redoneHeaders);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Moving_A_Column_Left_Puts_It_In_Front_Of_Its_Neighbour()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            using var uiThread = new InlineUiThread();

            await WriteThroughSinkAsync(factory, path, ThreeTextColumns(), [["a1", "b1", "c1"]]);

            var source = new TableStoreColumnsDataSource(factory, history) { Path = path };

            var columnC = await FindColumnAsync(source, "C");

            Assert.NotNull(columnC);

            Assert.True(await source.MoveAsync(columnC, -1));

            var (movedHeaders, movedRows) = await ReadTableAsync(factory, path);

            Assert.Equal(["A", "C", "B"], movedHeaders);
            AssertRow(["a1", "c1", "b1"], movedRows[0]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Column_At_The_Edge_Is_Left_Where_It_Is()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            using var uiThread = new InlineUiThread();

            await WriteThroughSinkAsync(factory, path, ThreeTextColumns(), [["a1", "b1", "c1"]]);

            var source = new TableStoreColumnsDataSource(factory, history) { Path = path };

            var first = await FindColumnAsync(source, "A");
            var last = await FindColumnAsync(source, "C");

            Assert.NotNull(first);
            Assert.NotNull(last);

            // There is no place in front of the first column, nor behind the last, so neither move takes
            // place and neither leaves a step behind.
            Assert.False(await source.MoveAsync(first, -1));
            Assert.False(await source.MoveAsync(last, 1));

            Assert.Equal(["A", "B", "C"], (await ReadTableAsync(factory, path)).Headers);
            Assert.False(await history.CanUndoAsync(path));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Duplicating_A_Column_Adds_A_Copy_Beside_It_Holding_What_It_Holds()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            using var uiThread = new InlineUiThread();

            await WriteThroughSinkAsync(factory, path, ThreeTextColumns(), [["a1", "b1", "c1"], ["a2", "b2", "c2"]]);

            var source = new TableStoreColumnsDataSource(factory, history) { Path = path };

            var columnB = await FindColumnAsync(source, "B");

            Assert.NotNull(columnB);

            var copy = await source.DuplicateAsync(columnB);

            Assert.NotNull(copy);

            var (copiedHeaders, copiedRows) = await ReadTableAsync(factory, path);

            // The copy lands beside the column it was taken from rather than at the end of the table, and
            // holds what that column holds.
            Assert.Equal(["A", "B", "B copy", "C"], copiedHeaders);
            AssertRow(["a1", "b1", "b1", "c1"], copiedRows[0]);
            AssertRow(["a2", "b2", "b2", "c2"], copiedRows[1]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Duplicated_Column_Is_Taken_Back_And_Put_Back()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            using var uiThread = new InlineUiThread();

            await WriteThroughSinkAsync(factory, path, ThreeTextColumns(), [["a1", "b1", "c1"]]);

            var source = new TableStoreColumnsDataSource(factory, history) { Path = path };

            var columnB = await FindColumnAsync(source, "B");

            Assert.NotNull(columnB);

            await source.DuplicateAsync(columnB);

            var undone = await history.UndoAsync(path);

            Assert.NotNull(undone);
            Assert.Equal("Duplicated the column 'B'", undone.Description);

            // The copy goes and the columns it made room for close up again, with the values they held.
            var (undoneHeaders, undoneRows) = await ReadTableAsync(factory, path);

            Assert.Equal(["A", "B", "C"], undoneHeaders);
            AssertRow(["a1", "b1", "c1"], undoneRows[0]);

            await history.RedoAsync(path);

            var (redoneHeaders, redoneRows) = await ReadTableAsync(factory, path);

            Assert.Equal(["A", "B", "B copy", "C"], redoneHeaders);
            AssertRow(["a1", "b1", "b1", "c1"], redoneRows[0]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Each_Copy_Of_A_Column_Is_Named_Apart_From_The_Ones_Before_It()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            using var uiThread = new InlineUiThread();

            await WriteThroughSinkAsync(factory, path, ThreeTextColumns(), [["a1", "b1", "c1"]]);

            var source = new TableStoreColumnsDataSource(factory, history) { Path = path };

            var columnB = await FindColumnAsync(source, "B");

            Assert.NotNull(columnB);

            var first = await source.DuplicateAsync(columnB);

            var second = await source.DuplicateAsync(columnB);

            Assert.NotNull(first);
            Assert.NotNull(second);

            // A copy named the same as one already there would read as the same column, so each copy is
            // given a name the table does not carry.
            Assert.Equal("B copy", first.Name);
            Assert.Equal("B copy 2", second.Name);
            Assert.Equal(["A", "B", "B copy 2", "B copy", "C"], (await ReadTableAsync(factory, path)).Headers);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Column_Added_Without_A_Name_Is_Given_One_The_Table_Does_Not_Carry()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            using var uiThread = new InlineUiThread();

            await WriteThroughSinkAsync(factory, path, ThreeTextColumns(), [["a1", "b1", "c1"]]);

            var source = new TableStoreColumnsDataSource(factory, history) { Path = path };

            // A column added before it has been named still has to read as a column of the table, so it is
            // given a name the table does not already hold rather than left nameless.
            await source.CreateAsync(new ColumnEntity
            {
                Name = string.Empty,
                DataType = ColumnDataType.Text,
                DefaultValueForCell = string.Empty,
            });

            var (headers, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(["A", "B", "C", "Column 1"], headers);
            AssertRow(["a1", "b1", "c1", null], rows[0]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task An_Unnamed_Column_Skips_A_Name_The_Table_Already_Carries()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            using var uiThread = new InlineUiThread();

            await WriteThroughSinkAsync(
                factory,
                path,
                [new TableColumn("Column 1", ColumnDataType.Text), new TableColumn("Other", ColumnDataType.Text)],
                [["a", "b"]]);

            var source = new TableStoreColumnsDataSource(factory, history) { Path = path };

            await source.CreateAsync(new ColumnEntity
            {
                Name = "   ",
                DataType = ColumnDataType.Text,
                DefaultValueForCell = string.Empty,
            });

            Assert.Equal(["Column 1", "Other", "Column 2"], (await ReadTableAsync(factory, path)).Headers);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Renaming_A_Column_To_A_Name_Already_Taken_Is_Refused()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            using var uiThread = new InlineUiThread();

            await WriteThroughSinkAsync(factory, path, ThreeTextColumns(), [["a1", "b1", "c1"]]);

            var source = new TableStoreColumnsDataSource(factory, history) { Path = path };

            var rejections = new List<string>();
            source.EditRejected += (_, args) => rejections.Add(args.Reason);

            var columnB = await FindColumnAsync(source, "B");

            Assert.NotNull(columnB);

            columnB.Name = "A";

            await source.UpdateAsync(columnB);

            // Two columns sharing a name would make every row that reads it ambiguous, so the rename is
            // refused and the table is left as it was.
            Assert.Equal(["A", "B", "C"], (await ReadTableAsync(factory, path)).Headers);
            Assert.Contains(rejections, reason => reason.Contains("already called 'A'"));
            Assert.False(await history.CanUndoAsync(path));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Renaming_A_Column_To_Nothing_Is_Refused()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            using var uiThread = new InlineUiThread();

            await WriteThroughSinkAsync(factory, path, ThreeTextColumns(), [["a1", "b1", "c1"]]);

            var source = new TableStoreColumnsDataSource(factory, history) { Path = path };

            var rejections = new List<string>();
            source.EditRejected += (_, args) => rejections.Add(args.Reason);

            var columnB = await FindColumnAsync(source, "B");

            Assert.NotNull(columnB);

            columnB.Name = "   ";

            await source.UpdateAsync(columnB);

            Assert.Equal(["A", "B", "C"], (await ReadTableAsync(factory, path)).Headers);
            Assert.Contains(rejections, reason => reason.Contains("needs a name"));
            Assert.False(await history.CanUndoAsync(path));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task The_Last_Column_Cannot_Be_Removed()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            using var uiThread = new InlineUiThread();

            await WriteThroughSinkAsync(
                factory,
                path,
                [new TableColumn("Only", ColumnDataType.Text)],
                [["v1"]]);

            var source = new TableStoreColumnsDataSource(factory, history) { Path = path };

            var rejections = new List<string>();
            source.EditRejected += (_, args) => rejections.Add(args.Reason);

            var only = await FindColumnAsync(source, "Only");

            Assert.NotNull(only);

            await source.DeleteAsync(only);

            // A table has to keep a column for its rows to be read by, so the last one is kept back.
            Assert.Equal(["Only"], (await ReadTableAsync(factory, path)).Headers);
            Assert.Contains(rejections, reason => reason.Contains("at least one column"));
            Assert.False(await history.CanUndoAsync(path));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task The_Columns_Are_Read_In_The_Order_The_Store_Keeps_Them_In_Even_After_A_Column_Is_Added()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            using var uiThread = new InlineUiThread();

            await WriteThroughSinkAsync(factory, path, ThreeTextColumns(), [["a1", "b1", "c1"]]);

            var source = new TableStoreColumnsDataSource(factory, history) { Path = path };

            // Adding a column is what the editor offers beside moving one, and it is the operation that
            // takes the ordering away from the filter the data source was carrying it in.
            await source.CreateAsync(new ColumnEntity
            {
                Name = string.Empty,
                DataType = ColumnDataType.Text,
                DefaultValueForCell = string.Empty,
            });

            var columnB = await FindColumnAsync(source, "B");

            Assert.NotNull(columnB);

            Assert.True(await source.MoveAsync(columnB, 1));

            // The editor shows the columns in the places they hold, so the read the grid is filled from has
            // to come back in that order. B and C changed places, so B has to be read after C - reading the
            // columns in the order of their ids would put B back in front and the move would show nowhere.
            var names = (await source.GetModelsAtAsync(0, 20)).Select(column => column.Name).ToList();

            Assert.Equal(["A", "C", "B", "Column 1"], names);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Column_Statistics_Count_Blanks_And_Distinct_Values()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory,
                path,
                [new TableColumn("Name", ColumnDataType.Text), new TableColumn("Amount", ColumnDataType.Integer)],
                [["x", "1"], ["x", "2"], [null, "3"], ["y", null]]);

            await using var db = await factory.CreateDbContextAsync(path);

            var statistics = await TableStoreStatistics.ReadAsync(db);

            Assert.Equal(4, statistics.RowCount);
            Assert.Equal(2, statistics.ColumnCount);

            var name = statistics.Columns.Single(column => column.Name == "Name");
            var amount = statistics.Columns.Single(column => column.Name == "Amount");

            // The row with nothing in it reads as a blank against both columns, and the values that are
            // there are counted once each however often they appear.
            Assert.Equal(3, name.FilledCount);
            Assert.Equal(1, name.EmptyCount);
            Assert.Equal(2, name.DistinctCount);

            Assert.Equal(3, amount.FilledCount);
            Assert.Equal(1, amount.EmptyCount);
            Assert.Equal(3, amount.DistinctCount);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Reading_The_Columns_Again_Tells_The_Grid_The_Items_It_Held_Are_Gone()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            using var uiThread = new InlineUiThread();

            await WriteThroughSinkAsync(factory, path, ThreeTextColumns(), [["a1", "b1", "c1"]]);

            var source = new TableStoreColumnsDataSource(factory, history) { Path = path };

            await source.EnsureInitialisedAsync();

            var notifications = new List<NotifyCollectionChangedAction>();

            void OnChanged(object? sender, NotifyCollectionChangedEventArgs args) => notifications.Add(args.Action);

            source.Collection.CollectionChanged += OnChanged;

            try
            {
                // Reading the columns again is what the tool does when the store refuses an edit. The reset
                // it raises is what tells the selection that the items it was holding were thrown away, so
                // the tool's own guard can drop them rather than leave the grid pointing at columns the
                // store no longer has. This is the contract that guard rests on.
                source.Invalidate();

                Assert.Contains(NotifyCollectionChangedAction.Reset, notifications);
            }
            finally
            {
                source.Collection.CollectionChanged -= OnChanged;
            }
        }
        finally
        {
            DeleteStore(path);
        }
    }
}
