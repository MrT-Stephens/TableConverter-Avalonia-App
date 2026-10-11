using Microsoft.EntityFrameworkCore;
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

namespace TableConverter.Tests.History;

public class ColumnSourceHistoryTests
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

    [Fact]
    public async Task A_Column_Added_Through_The_Source_Is_Taken_Back_On_Its_Own()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            using var uiThread = new InlineUiThread();

            await WriteThroughSinkAsync(factory, path, ["A", "B"], [["a1", "b1"], ["a2", "b2"]]);

            var source = new TableStoreColumnsDataSource(factory, history) { Path = path };

            var (created, _, _) = await source.CreateAsync(new ColumnEntity
            {
                Name = "C",
                DataType = ColumnDataType.Text,
                DefaultValueForCell = string.Empty,
            });

            Assert.True(created);

            var (addedHeaders, addedRows) = await ReadTableAsync(factory, path);

            Assert.Equal(["A", "B", "C"], addedHeaders);
            Assert.Equal(2, addedRows.Count);
            Assert.Equal("a2", addedRows[1][0]);

            var undone = await history.UndoAsync(path);

            Assert.NotNull(undone);
            Assert.Equal("Added a column", undone.Description);

            // The columns that were already there keep their names, their places and the values under them:
            // only the one the step added is gone.
            var (undoneHeaders, undoneRows) = await ReadTableAsync(factory, path);

            Assert.Equal(["A", "B"], undoneHeaders);
            Assert.Equal(2, undoneRows.Count);
            Assert.Equal("a1", undoneRows[0][0]);
            Assert.Equal("b1", undoneRows[0][1]);
            Assert.Equal("a2", undoneRows[1][0]);
            Assert.Equal("b2", undoneRows[1][1]);

            await history.RedoAsync(path);

            var (redoneHeaders, _) = await ReadTableAsync(factory, path);

            Assert.Equal(["A", "B", "C"], redoneHeaders);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Column_Removed_Through_The_Source_Comes_Back_With_What_It_Held()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            using var uiThread = new InlineUiThread();

            await WriteThroughSinkAsync(factory, path, ["A", "B"], [["a1", "b1"]]);

            var source = new TableStoreColumnsDataSource(factory, history) { Path = path };

            var columnB = await source.GetItemAsync(column => column.Name == "B");

            Assert.NotNull(columnB);

            Assert.True(await source.DeleteAsync(columnB));

            var (removedHeaders, removedRows) = await ReadTableAsync(factory, path);

            Assert.Equal(["A"], removedHeaders);
            Assert.Equal("a1", removedRows[0][0]);

            await history.UndoAsync(path);

            // The column returns where it was, holding the value that was under it when it went.
            var (undoneHeaders, undoneRows) = await ReadTableAsync(factory, path);

            Assert.Equal(["A", "B"], undoneHeaders);
            Assert.Equal("a1", undoneRows[0][0]);
            Assert.Equal("b1", undoneRows[0][1]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Column_Renamed_Through_The_Source_Is_Taken_Back_To_Its_Old_Name()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            using var uiThread = new InlineUiThread();

            await WriteThroughSinkAsync(factory, path, ["A", "B"], [["a1", "b1"]]);

            var source = new TableStoreColumnsDataSource(factory, history) { Path = path };

            var columnB = await source.GetItemAsync(column => column.Name == "B");

            Assert.NotNull(columnB);

            columnB.Name = "Renamed";

            await source.UpdateAsync(columnB);

            Assert.Equal(["A", "Renamed"], (await ReadTableAsync(factory, path)).Headers);

            var undone = await history.UndoAsync(path);

            Assert.NotNull(undone);
            Assert.Equal("Edited a column", undone.Description);

            // A rename changes what the column is called and nothing else, so the values under it are left
            // where they are.
            var (undoneHeaders, undoneRows) = await ReadTableAsync(factory, path);

            Assert.Equal(["A", "B"], undoneHeaders);
            Assert.Equal("a1", undoneRows[0][0]);
            Assert.Equal("b1", undoneRows[0][1]);
        }
        finally
        {
            DeleteStore(path);
        }
    }
}
