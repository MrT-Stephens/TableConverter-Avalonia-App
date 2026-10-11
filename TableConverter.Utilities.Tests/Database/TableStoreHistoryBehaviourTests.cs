using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.Extensions;
using TableConverter.Utilities.Database.History;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities.Tests.Database;

/// <summary>
/// Covers how the history behaves around the steps themselves: what it tells whoever is watching it, how
/// it is trimmed, that one store's history is its own, and that a step which cannot be read or was never
/// committed is handled rather than trusted.
/// </summary>
public class TableStoreHistoryBehaviourTests
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

    private static async Task<List<int>> ReadRowIdsAsync(ITableStoreDbContextFactory factory, string path)
    {
        await using var dbContext = await factory.CreateDbContextAsync(path);

        return await dbContext.Rows
            .AsNoTracking()
            .OrderBy(row => row.Id)
            .Select(row => row.Id)
            .ToListAsync();
    }

    private static async Task<int> CountRowsAsync(ITableStoreDbContextFactory factory, string path)
    {
        await using var dbContext = await factory.CreateDbContextAsync(path);

        return await dbContext.Rows.AsNoTracking().CountAsync();
    }

    private static async Task RemoveRowsAsync(
        ITableStoreDbContextFactory factory,
        string path,
        IReadOnlyCollection<int> rowIds)
    {
        await using var dbContext = await factory.CreateDbContextAsync(path);

        var ids = rowIds.ToArray();

        await dbContext.Cells.Where(cell => ids.Contains(cell.RowId)).ExecuteDeleteAsync();
        await dbContext.Rows.Where(row => ids.Contains(row.Id)).ExecuteDeleteAsync();
    }

    private static async Task SetCellAsync(
        ITableStoreDbContextFactory factory,
        string path,
        int rowId,
        int columnId,
        string? value)
    {
        await using var dbContext = await factory.CreateDbContextAsync(path);

        var cell = await dbContext.Cells.SingleAsync(cell => cell.RowId == rowId && cell.ColumnId == columnId);

        cell.Value = value;

        await dbContext.SaveChangesAsync();
    }

    private static async Task<Dictionary<string, int>> ReadColumnIdsAsync(
        ITableStoreDbContextFactory factory,
        string path)
    {
        await using var dbContext = await factory.CreateDbContextAsync(path);

        return await dbContext.Columns
            .AsNoTracking()
            .ToDictionaryAsync(column => column.Name, column => column.Id);
    }

    /// <summary>
    /// Records the removal of one row.
    /// </summary>
    private static async Task RecordRemovingOneRowAsync(
        ITableStoreDbContextFactory factory,
        ITableHistory history,
        string path)
    {
        var rowIds = await ReadRowIdsAsync(factory, path);
        var removed = rowIds[^1];

        await using var edit = history.BeginEdit(path, TableEditKind.RowsDeleted, "Removed 1 row(s)");

        await edit.CaptureBeforeAsync(TableRegion.Rows([removed]));

        await RemoveRowsAsync(factory, path, [removed]);

        Assert.NotNull(await edit.CommitAsync());
    }

    [Fact]
    public async Task Recording_A_Step_Tells_Watchers_What_Was_Recorded()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        var events = new List<HistoryChangedEventArgs>();
        history.Changed += (_, args) => events.Add(args);

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A"], [["a1"], ["a2"]]);

            await RecordRemovingOneRowAsync(factory, history, path);

            var raised = Assert.Single(events);

            Assert.Equal(path, raised.Path);
            Assert.Equal(HistoryAction.Recorded, raised.Action);

            Assert.NotNull(raised.Entry);
            Assert.Equal("Removed 1 row(s)", raised.Entry.Description);
            Assert.Equal(TableEditKind.RowsDeleted, raised.Entry.Kind);
            Assert.True(raised.Entry.IsApplied);
            Assert.Equal(1, raised.Entry.Sequence);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Taking_A_Step_Back_And_Putting_It_In_Place_Again_Is_Told_To_Watchers()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        var events = new List<HistoryChangedEventArgs>();
        history.Changed += (_, args) => events.Add(args);

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A"], [["a1"], ["a2"]]);
            await RecordRemovingOneRowAsync(factory, history, path);

            events.Clear();

            var undone = await history.UndoAsync(path);

            var undoneEvent = Assert.Single(events);

            Assert.Equal(HistoryAction.Undone, undoneEvent.Action);
            Assert.Equal(path, undoneEvent.Path);
            Assert.Same(undone, undoneEvent.Entry);
            Assert.NotNull(undoneEvent.Entry);
            Assert.False(undoneEvent.Entry.IsApplied);

            events.Clear();

            var redone = await history.RedoAsync(path);

            var redoneEvent = Assert.Single(events);

            Assert.Equal(HistoryAction.Redone, redoneEvent.Action);
            Assert.Same(redone, redoneEvent.Entry);
            Assert.NotNull(redoneEvent.Entry);
            Assert.True(redoneEvent.Entry.IsApplied);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Throwing_The_History_Away_Is_Told_To_Watchers_And_Leaves_The_Table()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        var events = new List<HistoryChangedEventArgs>();
        history.Changed += (_, args) => events.Add(args);

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A"], [["a1"], ["a2"]]);
            await RecordRemovingOneRowAsync(factory, history, path);

            events.Clear();

            await history.ClearAsync(path);

            var raised = Assert.Single(events);

            Assert.Equal(HistoryAction.Cleared, raised.Action);
            Assert.Equal(path, raised.Path);

            // Throwing the history away forgets how the table got here rather than putting it back where
            // it started, so the cleared entry names no step.
            Assert.Null(raised.Entry);

            Assert.Empty(await history.GetEntriesAsync(path));
            Assert.False(await history.CanUndoAsync(path));
            Assert.Equal(1, await CountRowsAsync(factory, path));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Throwing_Away_A_History_That_Holds_Nothing_Tells_Nobody()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        var events = new List<HistoryChangedEventArgs>();
        history.Changed += (_, args) => events.Add(args);

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A"], [["a1"]]);

            await history.ClearAsync(path);

            Assert.Empty(events);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task An_Operation_That_Changes_Nothing_Tells_Nobody()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        var events = new List<HistoryChangedEventArgs>();
        history.Changed += (_, args) => events.Add(args);

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A"], [["a1"]]);

            var rowIds = await ReadRowIdsAsync(factory, path);

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await using var edit = history.BeginEdit(path, TableEditKind.CellsChanged, "Trimmed whitespace");

                await edit.CaptureBeforeAsync(TableRegion.Rows(rowIds));

                // Nothing is changed, so nothing is recorded and nobody is told about it.
                Assert.Null(await edit.CommitAsync());
            }

            Assert.Empty(events);
            Assert.Empty(await history.GetEntriesAsync(path));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Taking_A_Step_Back_When_There_Is_None_Tells_Nobody()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        var events = new List<HistoryChangedEventArgs>();
        history.Changed += (_, args) => events.Add(args);

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A"], [["a1"]]);

            Assert.Null(await history.UndoAsync(path));
            Assert.Null(await history.RedoAsync(path));

            Assert.Empty(events);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public void The_Maximum_Number_Of_Steps_Kept_Is_At_Least_One()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var logger = provider.GetRequiredService<ILogger<TableStoreHistory>>();

        // A history that kept nothing would be a history that does nothing, so the number is clamped
        // rather than honoured blindly.
        Assert.Equal(1, new TableStoreHistory(factory, logger, maximumEntries: 0).MaximumEntries);
        Assert.Equal(1, new TableStoreHistory(factory, logger, maximumEntries: -10).MaximumEntries);
        Assert.Equal(5, new TableStoreHistory(factory, logger, maximumEntries: 5).MaximumEntries);
        Assert.Equal(
            TableStoreHistory.DefaultMaximumEntries,
            new TableStoreHistory(factory, logger).MaximumEntries);
    }

    [Fact]
    public async Task A_New_Step_After_A_Step_Was_Taken_Back_Drops_What_Could_Have_Been_Redone()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A"], [["a1"]]);

            await using var db = await factory.CreateDbContextAsync(path);

            await using (var edit = history.BeginEdit(path, TableEditKind.RowsAdded, "Added row 1"))
            {
                var rowId = await TableStoreMaintenance.Create(db).AddRowAsync();
                await edit.CaptureAfterAsync(TableRegion.Rows([rowId]));
                Assert.NotNull(await edit.CommitAsync());
            }

            await using (var edit = history.BeginEdit(path, TableEditKind.RowsAdded, "Added row 2"))
            {
                var rowId = await TableStoreMaintenance.Create(db).AddRowAsync();
                await edit.CaptureAfterAsync(TableRegion.Rows([rowId]));
                Assert.NotNull(await edit.CommitAsync());
            }

            await history.UndoAsync(path);

            Assert.True(await history.CanRedoAsync(path));

            await using (var edit = history.BeginEdit(path, TableEditKind.RowsAdded, "Added row 3"))
            {
                var rowId = await TableStoreMaintenance.Create(db).AddRowAsync();
                await edit.CaptureAfterAsync(TableRegion.Rows([rowId]));
                Assert.NotNull(await edit.CommitAsync());
            }

            Assert.False(await history.CanRedoAsync(path));

            var entries = await history.GetEntriesAsync(path);

            // The step that was taken back described a table that has since gone a different way, so it is
            // dropped rather than left in front of a cursor that no longer points at it.
            Assert.Equal(["Added row 1", "Added row 3"], entries.Select(entry => entry.Description));

            // The sequence keeps rising across the gap the dropped step left, so a step never sorts itself
            // in front of one taken earlier.
            Assert.Equal([1, 3], entries.Select(entry => entry.Sequence));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task The_Oldest_Steps_Are_Dropped_And_Can_No_Longer_Be_Taken_Back()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var history = new TableStoreHistory(
            factory,
            provider.GetRequiredService<ILogger<TableStoreHistory>>(),
            maximumEntries: 2);

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A"], [["a1"]]);

            await using var db = await factory.CreateDbContextAsync(path);

            for (var step = 1; step <= 3; step++)
            {
                await using var edit = history.BeginEdit(path, TableEditKind.RowsAdded, $"Added row {step}");

                var rowId = await TableStoreMaintenance.Create(db).AddRowAsync();

                await edit.CaptureAfterAsync(TableRegion.Rows([rowId]));

                Assert.NotNull(await edit.CommitAsync());
            }

            var entries = await history.GetEntriesAsync(path);

            Assert.Equal(["Added row 2", "Added row 3"], entries.Select(entry => entry.Description));
            Assert.Equal(2, await history.GetUndoDepthAsync(path));

            // The first step has been forgotten, so the table can only be walked back as far as the steps
            // that are still held.
            Assert.NotNull(await history.UndoAsync(path));
            Assert.NotNull(await history.UndoAsync(path));
            Assert.Null(await history.UndoAsync(path));

            Assert.Equal(2, await CountRowsAsync(factory, path));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_History_Belongs_To_One_Store_And_Not_To_Another()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var first = NewStorePath();
        var second = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, first, ["A"], [["a1"], ["a2"]]);
            await WriteThroughSinkAsync(factory, second, ["A"], [["b1"], ["b2"]]);

            await RecordRemovingOneRowAsync(factory, history, first);

            Assert.Single(await history.GetEntriesAsync(first));
            Assert.Empty(await history.GetEntriesAsync(second));
            Assert.False(await history.CanUndoAsync(second));

            // Taking a step back in one store leaves the other exactly as it was.
            await history.UndoAsync(first);

            Assert.Equal(2, await CountRowsAsync(factory, first));
            Assert.Equal(2, await CountRowsAsync(factory, second));
        }
        finally
        {
            DeleteStore(first);
            DeleteStore(second);
        }
    }

    [Fact]
    public async Task What_Could_Be_Redone_Outlives_The_Document_That_Wrote_It()
    {
        var path = NewStorePath();

        try
        {
            using (var provider = BuildProvider())
            {
                var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
                var history = provider.GetRequiredService<ITableHistory>();

                await WriteThroughSinkAsync(factory, path, ["A"], [["a1"], ["a2"]]);

                await RecordRemovingOneRowAsync(factory, history, path);

                await history.UndoAsync(path);

                Assert.True(await history.CanRedoAsync(path));
            }

            // Everything the first document knew has been let go of, so what can be put back has to come
            // from the store file.
            using (var provider = BuildProvider())
            {
                var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
                var history = provider.GetRequiredService<ITableHistory>();

                Assert.True(await history.CanRedoAsync(path));

                var entries = await history.GetEntriesAsync(path);

                var entry = Assert.Single(entries);

                Assert.False(entry.IsApplied);

                Assert.NotNull(await history.RedoAsync(path));
                Assert.False(await history.CanRedoAsync(path));
                Assert.Equal(1, await CountRowsAsync(factory, path));
            }
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Step_That_Cannot_Be_Read_Stops_The_Walk_Rather_Than_Losing_The_Place()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A"], [["a1"], ["a2"]]);

            await RecordRemovingOneRowAsync(factory, history, path);

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                // A step whose payload cannot be read, which is what a store written by a build that knew
                // a kind of step this one does not would look like.
                await db.Database.ExecuteSqlRawAsync("UPDATE TABLE_HISTORY SET PAYLOAD = 'not json';");
            }

            Assert.Null(await history.UndoAsync(path));

            // The cursor does not move, so the step is still there to be undone once the payload can be
            // read; a step that failed part way is not recorded as having been taken back.
            Assert.Equal(1, await history.GetUndoDepthAsync(path));

            var entry = Assert.Single(await history.GetEntriesAsync(path));

            Assert.True(entry.IsApplied);
            Assert.Equal(1, await CountRowsAsync(factory, path));

            // The history can still be thrown away, so a store holding a step this build cannot read is
            // not one that cannot be cleaned up.
            await history.ClearAsync(path);

            Assert.Empty(await history.GetEntriesAsync(path));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public void Recording_Against_A_Blank_Path_Is_Rejected()
    {
        using var provider = BuildProvider();
        var history = provider.GetRequiredService<ITableHistory>();

        Assert.Throws<ArgumentException>(
            () => history.BeginEdit("  ", TableEditKind.RowsAdded, "Added a row"));
    }

    [Fact]
    public async Task A_Recording_That_Is_Committed_Twice_Writes_One_Step()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A"], [["a1"]]);

            var columns = await ReadColumnIdsAsync(factory, path);
            var rowIds = await ReadRowIdsAsync(factory, path);

            await using var edit = history.BeginEdit(path, TableEditKind.CellsChanged, "Changed a cell");

            await edit.CaptureBeforeAsync(TableRegion.Cells([(rowIds[0], columns["A"])]));

            await SetCellAsync(factory, path, rowIds[0], columns["A"], "a1x");

            Assert.NotNull(await edit.CommitAsync());

            // Reaching the commit a second time must not write the step again: the operation happened
            // once, so it is recorded once.
            Assert.Null(await edit.CommitAsync());

            Assert.Single(await history.GetEntriesAsync(path));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Recording_With_Nothing_Described_Writes_No_Step()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A"], [["a1"]]);

            await using var edit = history.BeginEdit(path, TableEditKind.RowsAdded, "Added a row");

            Assert.Null(await edit.CommitAsync());
            Assert.Empty(await history.GetEntriesAsync(path));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Recording_That_Is_Given_Up_On_Leaves_The_Next_One_FREE_TO_RUN()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A"], [["a1"], ["a2"]]);

            var rowIds = await ReadRowIdsAsync(factory, path);

            await using (var abandoned = history.BeginEdit(path, TableEditKind.RowsDeleted, "Removed 1 row(s)"))
            {
                await abandoned.CaptureBeforeAsync(TableRegion.Rows([rowIds[0]]));

                // The operation fails, so the recording is disposed without ever being committed.
            }

            Assert.Empty(await history.GetEntriesAsync(path));

            var columns = await ReadColumnIdsAsync(factory, path);

            await using (var edit = history.BeginEdit(path, TableEditKind.CellsChanged, "Changed a cell"))
            {
                await edit.CaptureBeforeAsync(TableRegion.Cells([(rowIds[0], columns["A"])]));
                await SetCellAsync(factory, path, rowIds[0], columns["A"], "a1x");
                Assert.NotNull(await edit.CommitAsync());
            }

            Assert.Single(await history.GetEntriesAsync(path));

            // The row the abandoned recording described is still there, because nothing about giving up
            // touched the table.
            Assert.Equal(2, await CountRowsAsync(factory, path));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Store_Without_A_History_TABLE_GAINS_ONE_WHEN_A_STEP_IS_TAKEN()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A"], [["a1"], ["a2"]]);

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                // What a store written by a version without a history looks like.
                await db.Database.ExecuteSqlRawAsync("DROP TABLE TABLE_HISTORY;");
            }

            // The table a store predating the history is missing has to be added rather than assumed, or
            // the first step taken on an old store would fail.
            await RecordRemovingOneRowAsync(factory, history, path);

            Assert.Single(await history.GetEntriesAsync(path));

            Assert.NotNull(await history.UndoAsync(path));
            Assert.Equal(2, await CountRowsAsync(factory, path));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task An_Entry_Reports_When_It_Was_Taken()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A"], [["a1"], ["a2"]]);

            var started = DateTimeOffset.UtcNow.AddSeconds(-5);

            await RecordRemovingOneRowAsync(factory, history, path);

            var finished = DateTimeOffset.UtcNow.AddSeconds(5);

            var entry = Assert.Single(await history.GetEntriesAsync(path));

            // A step is stamped in seconds since the Unix epoch, which is the one time representation
            // every provider agrees on where they have no date type of their own.
            Assert.InRange(entry.Timestamp, started, finished);
        }
        finally
        {
            DeleteStore(path);
        }
    }
}

