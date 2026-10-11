using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Extensions;
using TableConverter.Utilities.Database.History;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities.Tests.Database;

/// <summary>
/// Covers the edit history a store keeps of itself: how an operation is recorded, how a step is taken
/// back and put in place again, and what is left in the store once the document that made the step has
/// been closed.
/// </summary>
/// <remarks>
/// The tests record edits the way the application does, by describing the part of the table an operation
/// is about to change and then committing the recording once the operation has run. What matters is the
/// table the store is left holding rather than the statement that changed it.
/// </remarks>
public class TableStoreHistoryTests
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

    private static async Task<List<int>> ReadRowIdsAsync(ITableStoreDbContextFactory factory, string path)
    {
        await using var dbContext = await factory.CreateDbContextAsync(path);

        return await dbContext.Rows
            .AsNoTracking()
            .OrderBy(row => row.Id)
            .Select(row => row.Id)
            .ToListAsync();
    }

    /// <summary>
    /// Removes rows the way the delete rows command does, so what is recorded is recorded against a
    /// removal made with statements rather than through the change tracker.
    /// </summary>
    private static async Task RemoveRowsAsync(TableStoreDbContext db, IReadOnlyCollection<int> rowIds)
    {
        var ids = rowIds.ToArray();

        await db.Cells.Where(cell => ids.Contains(cell.RowId)).ExecuteDeleteAsync();
        await db.Rows.Where(row => ids.Contains(row.Id)).ExecuteDeleteAsync();
    }

    [Fact]
    public async Task Removing_Rows_Is_Undone_With_Their_Ids_And_Their_Values()
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
                ["A", "B"],
                [
                    ["a1", "b1"],
                    ["a2", "b2"],
                    ["a3", "b3"],
                ]);

            var rowIds = await ReadRowIdsAsync(factory, path);

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await using var edit = history.BeginEdit(path, TableEditKind.RowsDeleted, "Removed 1 row(s)");

                await edit.CaptureBeforeAsync(TableRegion.Rows([rowIds[1]]));

                await RemoveRowsAsync(db, [rowIds[1]]);

                Assert.NotNull(await edit.CommitAsync());
            }

            var (_, remaining) = await ReadTableAsync(factory, path);

            Assert.Equal(["a1", "b1"], remaining[0]);
            Assert.Equal(["a3", "b3"], remaining[1]);

            var undone = await history.UndoAsync(path);

            Assert.NotNull(undone);
            Assert.Equal(TableEditKind.RowsDeleted, undone.Kind);
            Assert.False(undone.IsApplied);

            var (_, restored) = await ReadTableAsync(factory, path);

            // The row comes back under the id it was removed under, which is the place the table keeps
            // it in, so it lands between the two rows it sat between rather than at the end.
            Assert.Equal(["a1", "b1"], restored[0]);
            Assert.Equal(["a2", "b2"], restored[1]);
            Assert.Equal(["a3", "b3"], restored[2]);
            Assert.Equal(rowIds, await ReadRowIdsAsync(factory, path));

            await history.RedoAsync(path);

            var (_, removedAgain) = await ReadTableAsync(factory, path);

            Assert.Equal(2, removedAgain.Count);
            Assert.Equal(["a1", "b1"], removedAgain[0]);
            Assert.Equal(["a3", "b3"], removedAgain[1]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Trimming_Is_Undone_With_The_Values_And_Names_That_Were_There_Before()
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
                [" A ", "B"],
                [
                    [" a1 ", "b1 "],
                    ["a2", "\t "],
                ]);

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                var maintenance = TableStoreMaintenance.Create(db);

                await using var edit = history.BeginEdit(
                    path, TableEditKind.Unknown, "Trimmed whitespace");

                // A trim rewrites cells and column names alike, so both are described before it runs and
                // the two readings are recorded as one step the user thinks of as one.
                await edit.CaptureBeforeAsync(TableRegion.Rows(await maintenance.GetRowIdsToTrimAsync()));
                await edit.CaptureBeforeAsync(TableRegion.Columns());

                Assert.Equal(3, await maintenance.TrimAsync());

                var entry = await edit.CommitAsync();

                Assert.NotNull(entry);
                Assert.Equal(TableEditKind.Composite, entry.Kind);
            }

            var (headers, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(["A", "B"], headers);
            Assert.Equal(["a1", "b1"], rows[0]);
            Assert.Equal(["a2", ""], rows[1]);

            await history.UndoAsync(path);

            (headers, rows) = await ReadTableAsync(factory, path);

            // The padding, the names and the value that was nothing but whitespace are all back exactly
            // as they were, which is what makes undoing a trim worth having.
            Assert.Equal([" A ", "B"], headers);
            Assert.Equal([" a1 ", "b1 "], rows[0]);
            Assert.Equal(["a2", "\t "], rows[1]);

            await history.RedoAsync(path);

            (headers, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(["A", "B"], headers);
            Assert.Equal(["a1", "b1"], rows[0]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Removing_Duplicate_Rows_Is_Undone()
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
                ["A", "B"],
                [
                    ["a1", "b1"],
                    ["a2", "b2"],
                    ["a1", "b1"],
                ]);

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                var maintenance = TableStoreMaintenance.Create(db);

                await using var edit = history.BeginEdit(
                    path, TableEditKind.RowsDeleted, "Removed duplicate rows");

                await edit.CaptureBeforeAsync(
                    TableRegion.Rows(await maintenance.GetDuplicateRowIdsAsync()));

                Assert.Equal(1, await maintenance.RemoveDuplicateRowsAsync());

                Assert.NotNull(await edit.CommitAsync());
            }

            var (_, deduplicated) = await ReadTableAsync(factory, path);

            Assert.Equal(2, deduplicated.Count);

            await history.UndoAsync(path);

            var (_, restored) = await ReadTableAsync(factory, path);

            Assert.Equal(3, restored.Count);
            Assert.Equal(["a1", "b1"], restored[0]);
            Assert.Equal(["a2", "b2"], restored[1]);
            Assert.Equal(["a1", "b1"], restored[2]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Removing_Empty_Rows_Is_Undone()
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
                ["A", "B"],
                [
                    ["a1", "b1"],
                    ["", ""],
                    ["a3", "b3"],
                ]);

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                var maintenance = TableStoreMaintenance.Create(db);

                await using var edit = history.BeginEdit(
                    path, TableEditKind.RowsDeleted, "Removed empty rows");

                await edit.CaptureBeforeAsync(TableRegion.Rows(await maintenance.GetEmptyRowIdsAsync()));

                Assert.Equal(1, await maintenance.RemoveEmptyRowsAsync());

                Assert.NotNull(await edit.CommitAsync());
            }

            var (_, emptied) = await ReadTableAsync(factory, path);

            Assert.Equal(2, emptied.Count);

            await history.UndoAsync(path);

            var (_, restored) = await ReadTableAsync(factory, path);

            Assert.Equal(3, restored.Count);
            Assert.Equal(["", ""], restored[1]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Adding_A_Row_Is_Undone()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A", "B"], [["a1", "b1"]]);

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await using var edit = history.BeginEdit(path, TableEditKind.RowsAdded, "Added a row");

                // A row that did not exist has no identity until it has been written, so it is described
                // by what it holds rather than by what it held.
                var rowId = await TableStoreMaintenance.Create(db).AddRowAsync();

                await edit.CaptureAfterAsync(TableRegion.Rows([rowId]));

                var entry = await edit.CommitAsync();

                // The kind is the one the operation declared, which is what the user ran rather than what
                // the payload happened to turn out to be.
                Assert.NotNull(entry);
                Assert.Equal(TableEditKind.RowsAdded, entry.Kind);
            }

            Assert.Equal(2, (await ReadTableAsync(factory, path)).Rows.Count);

            await history.UndoAsync(path);

            Assert.Single((await ReadTableAsync(factory, path)).Rows);

            await history.RedoAsync(path);

            Assert.Equal(2, (await ReadTableAsync(factory, path)).Rows.Count);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Turning_The_Table_Is_Undone_By_Turning_It_Back()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A", "B"], [["a1", "b1"], ["a2", "b2"]]);

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await using var edit = history.BeginEdit(
                    path, TableEditKind.TableRotated, "Transposed clockwise");

                await edit.CaptureBeforeAsync(TableRegion.Rotated(TableRotation.Clockwise));

                Assert.NotNull(await TableStoreMaintenance.Create(db).RotateAsync(TableRotation.Clockwise));

                Assert.NotNull(await edit.CommitAsync());
            }

            var (turnedHeaders, turnedRows) = await ReadTableAsync(factory, path);

            Assert.Equal(3, turnedHeaders.Count);
            Assert.Single(turnedRows);

            await history.UndoAsync(path);

            var (headers, rows) = await ReadTableAsync(factory, path);

            // A turn is undone by turning the other way, so the table comes back with the columns it had
            // and the headings it had rather than with a copy of them written back.
            Assert.Equal(["A", "B"], headers);
            Assert.Equal(2, rows.Count);
            Assert.Equal(["a1", "b1"], rows[0]);
            Assert.Equal(["a2", "b2"], rows[1]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Whole_Table_Step_Is_Undone_From_The_Snapshot_It_Kept()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A", "B"], [["a1", "b1"], ["a2", "b2"]]);

            var rowIds = await ReadRowIdsAsync(factory, path);

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await using var edit = history.BeginEdit(
                    path, TableEditKind.TableReplaced, "Replaced the table");

                // The whole table is described before it is replaced, which is the one kind of recording
                // whose size follows the table rather than the change.
                await edit.CaptureBeforeAsync(TableRegion.Table());

                await TableStoreMaintenance.Create(db).RotateAsync(TableRotation.CounterClockwise);

                var entry = await edit.CommitAsync();

                Assert.NotNull(entry);
                Assert.Equal(TableEditKind.TableReplaced, entry.Kind);
            }

            await history.UndoAsync(path);

            var (headers, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(["A", "B"], headers);
            Assert.Equal(2, rows.Count);
            Assert.Equal(["a1", "b1"], rows[0]);
            Assert.Equal(["a2", "b2"], rows[1]);

            // The columns and the rows come back under the ids they had, which is what stops a restored
            // table from being one the open grids no longer recognise.
            Assert.Equal(rowIds, await ReadRowIdsAsync(factory, path));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task An_Operation_That_Changes_Nothing_Leaves_No_Step()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A"], [["a1"]]);

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await using var edit = history.BeginEdit(
                    path, TableEditKind.CellsChanged, "Trimmed whitespace");

                await edit.CaptureBeforeAsync(TableRegion.Rows(await ReadRowIdsAsync(factory, path)));

                // The trim finds nothing to do, so the step that would describe it is not written.
                Assert.Equal(0, await TableStoreMaintenance.Create(db).TrimAsync());

                Assert.Null(await edit.CommitAsync());
            }

            Assert.Empty(await history.GetEntriesAsync(path));
            Assert.False(await history.CanUndoAsync(path));
            Assert.Null(await history.UndoAsync(path));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Recording_That_Is_Never_Committed_Leaves_No_Step()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A"], [["a1"]]);

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await using (var edit = history.BeginEdit(path, TableEditKind.RowsDeleted, "Removed 1 row(s)"))
                {
                    await edit.CaptureBeforeAsync(TableRegion.Rows(await ReadRowIdsAsync(factory, path)));

                    // The operation fails, so the recording is disposed without ever being committed.
                }
            }

            Assert.Empty(await history.GetEntriesAsync(path));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Editing_After_An_Undo_Discards_What_Could_Have_Been_Redone()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A"], [["a1"], ["a2"]]);

            var rowIds = await ReadRowIdsAsync(factory, path);

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await using var edit = history.BeginEdit(path, TableEditKind.RowsDeleted, "Removed 1 row(s)");

                await edit.CaptureBeforeAsync(TableRegion.Rows([rowIds[1]]));

                await RemoveRowsAsync(db, [rowIds[1]]);

                await edit.CommitAsync();
            }

            // The only step there is has just been taken, so there is nothing in front of the cursor that
            // could be put in place again.
            Assert.False(await history.CanRedoAsync(path));

            await history.UndoAsync(path);

            Assert.Equal(2, (await ReadTableAsync(factory, path)).Rows.Count);
            Assert.True(await history.CanRedoAsync(path));

            // A step is taken on a table that has been walked back, so the step that could have been put
            // in place again described a table that is no longer there.
            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await using var edit = history.BeginEdit(path, TableEditKind.RowsAdded, "Added a row");

                var rowId = await TableStoreMaintenance.Create(db).AddRowAsync();

                await edit.CaptureAfterAsync(TableRegion.Rows([rowId]));

                await edit.CommitAsync();
            }

            Assert.False(await history.CanRedoAsync(path));

            // The step that was taken back is discarded rather than left behind the cursor, so the only
            // step the history holds is the one just taken.
            Assert.Single(await history.GetEntriesAsync(path));
            Assert.Equal(3, (await ReadTableAsync(factory, path)).Rows.Count);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task The_History_Outlives_The_Document_That_Wrote_It()
    {
        var path = NewStorePath();

        try
        {
            using (var provider = BuildProvider())
            {
                var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
                var history = provider.GetRequiredService<ITableHistory>();

                await WriteThroughSinkAsync(factory, path, ["A"], [["a1"]]);

                await using var db = await factory.CreateDbContextAsync(path);

                await using var edit = history.BeginEdit(path, TableEditKind.RowsDeleted, "Removed 1 row(s)");

                var rowIds = await ReadRowIdsAsync(factory, path);

                await edit.CaptureBeforeAsync(TableRegion.Rows(rowIds));

                await RemoveRowsAsync(db, rowIds);

                await edit.CommitAsync();
            }

            // Everything the first document knew has been let go of, so the step coming back can only
            // come from the store file.
            using (var provider = BuildProvider())
            {
                var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
                var history = provider.GetRequiredService<ITableHistory>();

                var entries = await history.GetEntriesAsync(path);

                Assert.Single(entries);
                Assert.Equal("Removed 1 row(s)", entries[0].Description);
                Assert.True(entries[0].IsApplied);

                Assert.NotNull(await history.UndoAsync(path));
                Assert.Null(await history.UndoAsync(path));
                Assert.Single((await ReadTableAsync(factory, path)).Rows);
            }
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Only_The_Newest_Steps_Are_Kept()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var history = new TableStoreHistory(
            factory,
            provider.GetRequiredService<ILogger<TableStoreHistory>>(),
            maximumEntries: 3);

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A"], [["a1"]]);

            await using var db = await factory.CreateDbContextAsync(path);

            for (var step = 1; step <= 5; step++)
            {
                await using var edit = history.BeginEdit(
                    path, TableEditKind.RowsAdded, $"Added row {step}");

                var rowId = await TableStoreMaintenance.Create(db).AddRowAsync();

                await edit.CaptureAfterAsync(TableRegion.Rows([rowId]));

                await edit.CommitAsync();
            }

            var entries = await history.GetEntriesAsync(path);

            // A history that could never be trimmed would grow the store for as long as the table is
            // worked on, so the oldest steps are dropped once the store holds more than it keeps.
            Assert.Equal(3, entries.Count);
            Assert.Equal(["Added row 3", "Added row 4", "Added row 5"], entries.Select(entry => entry.Description));

            Assert.Equal(3, await history.GetUndoDepthAsync(path));

            // The six rows the five steps wrote are still there: dropping a step only forgets how to take
            // it back, it does not take it back.
            Assert.Equal(6, (await ReadTableAsync(factory, path)).Rows.Count);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Store_Written_Before_The_History_Existed_Is_Given_The_Table()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A"], [["a1"]]);

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                // What a store written by a version without a history looks like.
                await db.Database.ExecuteSqlRawAsync("DROP TABLE TABLE_HISTORY;");
            }

            // Opening the store is what adds the table back, because EnsureCreated only ever creates a
            // whole database and never changes one that is already there.
            var entries = await history.GetEntriesAsync(path);

            Assert.Empty(entries);
            Assert.False(await history.CanUndoAsync(path));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task The_Depth_Of_The_History_Follows_The_Cursor()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A"], [["a1"]]);

            await using var db = await factory.CreateDbContextAsync(path);

            for (var step = 0; step < 2; step++)
            {
                await using var edit = history.BeginEdit(path, TableEditKind.RowsAdded, "Added a row");

                var rowId = await TableStoreMaintenance.Create(db).AddRowAsync();

                await edit.CaptureAfterAsync(TableRegion.Rows([rowId]));

                await edit.CommitAsync();
            }

            Assert.Equal(2, await history.GetUndoDepthAsync(path));
            Assert.Equal(0, await history.GetRedoDepthAsync(path));

            await history.UndoAsync(path);

            Assert.Equal(1, await history.GetUndoDepthAsync(path));
            Assert.Equal(1, await history.GetRedoDepthAsync(path));

            await history.RedoAsync(path);

            Assert.Equal(2, await history.GetUndoDepthAsync(path));
            Assert.Equal(0, await history.GetRedoDepthAsync(path));

            await history.ClearAsync(path);

            Assert.Equal(0, await history.GetUndoDepthAsync(path));
            Assert.False(await history.CanUndoAsync(path));

            // Clearing the history leaves the table alone: it forgets how the table got here rather than
            // putting it back where it started.
            Assert.Equal(3, (await ReadTableAsync(factory, path)).Rows.Count);
        }
        finally
        {
            DeleteStore(path);
        }
    }
}

