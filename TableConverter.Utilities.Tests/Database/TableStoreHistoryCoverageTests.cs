using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.Extensions;
using TableConverter.Utilities.Database.History;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Utilities.Interfaces;
using TableConverter.Utilities.Models;

namespace TableConverter.Utilities.Tests.Database;

/// <summary>
/// Covers the history recordings that the operations of this build make but that the other history suites
/// do not reach: the whole table a sort remembers so its rows can be put back in the order they were in,
/// the column set an added column remembers so undo removes the one column rather than all of them, and a
/// handful of smaller behaviours around the kind, the notifications and the cursor.
/// </summary>
public class TableStoreHistoryCoverageTests
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

    private static async Task<List<string?>> ReadFirstColumnAsync(
        ITableStoreDbContextFactory factory,
        string path)
    {
        var (_, rows) = await ReadTableAsync(factory, path);

        return [.. rows.Select(row => row[0])];
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
    /// Renumbers rows so that the row that holds one thing now holds another, which is how the sort works:
    /// the order the store keeps its rows in is their ids, so ordering them means renumbering them.
    /// </summary>
    /// <param name="factory">The factory used to open the store.</param>
    /// <param name="path">The store to reorder.</param>
    /// <param name="currentOrder">The ids of the rows in the order they are in.</param>
    /// <param name="targetOrder">The id each of those rows is to be moved to.</param>
    private static async Task ReorderRowsAsync(
        ITableStoreDbContextFactory factory,
        string path,
        IReadOnlyList<int> currentOrder,
        IReadOnlyList<int> targetOrder)
    {
        await using var db = await factory.CreateDbContextAsync(path);

        var pairs = string.Join(",", currentOrder.Zip(targetOrder).Select(pair => $"({pair.First},{pair.Second})"));

        // Only integer ids this test read out of the store are inlined; there is nothing user supplied in
        // the statement.
#pragma warning disable EF1002
        await db.Database.ExecuteSqlRawAsync($"""
            DROP TABLE IF EXISTS T_TEST_ORDER;
            DROP TABLE IF EXISTS T_TEST_CELLS;

            CREATE TEMP TABLE T_TEST_ORDER (CURRENT_ID INTEGER PRIMARY KEY, TARGET_ID INTEGER NOT NULL);
            INSERT INTO T_TEST_ORDER (CURRENT_ID, TARGET_ID) VALUES {pairs};

            CREATE TEMP TABLE T_TEST_CELLS AS
                SELECT O.TARGET_ID AS ROW_ID, C.COLUMN_ID, C.VALUE
                FROM CELLS C JOIN T_TEST_ORDER O ON O.CURRENT_ID = C.ROW_ID;

            DELETE FROM CELLS;
            DELETE FROM ROWS;

            INSERT INTO ROWS (ID) SELECT TARGET_ID FROM T_TEST_ORDER ORDER BY TARGET_ID;

            INSERT INTO CELLS (ROW_ID, COLUMN_ID, VALUE)
                SELECT ROW_ID, COLUMN_ID, VALUE FROM T_TEST_CELLS ORDER BY ROW_ID, COLUMN_ID;

            DROP TABLE T_TEST_CELLS;
            DROP TABLE T_TEST_ORDER;
            """);
#pragma warning restore EF1002
    }

    [Fact]
    public async Task A_Sort_Of_A_Table_With_Contiguous_Ids_Is_Undone_And_Put_Back()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            // The ids a freshly imported table is given are 1, 2, 3 - the same ids a sort renumbers its
            // rows to - so a recording that remembered only the ids would see no change at all.
            await WriteThroughSinkAsync(factory, path, ["A"], [["b"], ["a"], ["c"]]);

            var rowIds = await ReadRowIdsAsync(factory, path);

            Assert.Equal([1, 2, 3], rowIds);

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await using var edit = history.BeginEdit(
                    path, TableEditKind.RowOrderChanged, "Sorted by 'A' ascending");

                // The whole table is remembered, because the values are what move between the ids and the
                // ids alone cannot say where they went.
                await edit.CaptureBeforeAsync(TableRegion.Table());

                await ReorderRowsAsync(factory, path, [rowIds[0], rowIds[1], rowIds[2]], [rowIds[1], rowIds[0], rowIds[2]]);

                var entry = await edit.CommitAsync();

                Assert.NotNull(entry);
                Assert.Equal(TableEditKind.RowOrderChanged, entry.Kind);
            }

            Assert.Equal(new List<string?> { "a", "b", "c" }, await ReadFirstColumnAsync(factory, path));
            Assert.Equal([1, 2, 3], await ReadRowIdsAsync(factory, path));

            var undone = await history.UndoAsync(path);

            Assert.NotNull(undone);
            Assert.Equal("Sorted by 'A' ascending", undone.Description);

            // The sort is taken back to the arrangement the rows were in, values and ids together.
            Assert.Equal(new List<string?> { "b", "a", "c" }, await ReadFirstColumnAsync(factory, path));

            await history.RedoAsync(path);

            Assert.Equal(new List<string?> { "a", "b", "c" }, await ReadFirstColumnAsync(factory, path));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Column_Added_And_Remembered_Before_It_Was_Is_Undone_And_Put_Back()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A", "B"], [["a1", "b1"]]);

            int addedColumnId;

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await using var edit = history.BeginEdit(path, TableEditKind.ColumnsChanged, "Added a column");

                // The set of columns is remembered before the new one is there. A set captured after the
                // add would leave the step describing an empty table as what came before it, so taking the
                // step back would take every column with it.
                await edit.CaptureBeforeAsync(TableRegion.Columns());

                var column = new ColumnEntity
                {
                    Name = "C",
                    DataType = ColumnDataType.Text,
                    OrdinalPosition = 3,
                };

                await db.Columns.AddAsync(column);
                await db.SaveChangesAsync();

                addedColumnId = column.Id;

                Assert.NotNull(await edit.CommitAsync());
            }

            var (addedHeaders, addedRows) = await ReadTableAsync(factory, path);

            Assert.Equal(["A", "B", "C"], addedHeaders);
            Assert.Equal("a1", addedRows[0][0]);
            Assert.Equal("b1", addedRows[0][1]);
            Assert.Null(addedRows[0][2]);

            await history.UndoAsync(path);

            var (undoneHeaders, undoneRows) = await ReadTableAsync(factory, path);

            // Only the added column goes: the columns that were already there keep their names, their
            // places and the values under them.
            Assert.Equal(["A", "B"], undoneHeaders);
            Assert.Equal("a1", undoneRows[0][0]);
            Assert.Equal("b1", undoneRows[0][1]);

            await history.RedoAsync(path);

            var (redoneHeaders, redoneRows) = await ReadTableAsync(factory, path);

            Assert.Equal(["A", "B", "C"], redoneHeaders);
            Assert.Equal("a1", redoneRows[0][0]);
            Assert.Equal("b1", redoneRows[0][1]);
            Assert.Null(redoneRows[0][2]);

            await using var check = await factory.CreateDbContextAsync(path);

            // The column comes back under the id it was added with, so a grid that is showing it is
            // looking at the same column rather than at a new one that merely reads the same.
            Assert.True(await check.Columns.AsNoTracking().AnyAsync(column => column.Id == addedColumnId));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Step_That_Names_No_Kind_Takes_The_Kind_Of_What_It_Changed()
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
                // An operation that does not name a kind is recorded by what its one payload turns out to
                // be, which is what lets an operation leave the naming to the change it made.
                await using var edit = history.BeginEdit(path, TableEditKind.Unknown, "Added a row");

                var rowId = await TableStoreMaintenance.Create(db).AddRowAsync();

                await edit.CaptureAfterAsync(TableRegion.Rows([rowId]));

                var entry = await edit.CommitAsync();

                Assert.NotNull(entry);
                Assert.Equal(TableEditKind.RowsAdded, entry.Kind);
            }
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Step_That_Cannot_Be_Read_Is_Not_Told_To_Watchers()
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

            var rowIds = await ReadRowIdsAsync(factory, path);

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await using var edit = history.BeginEdit(path, TableEditKind.RowsDeleted, "Removed 1 row(s)");

                await edit.CaptureBeforeAsync(TableRegion.Rows([rowIds[1]]));

                await db.Cells.Where(cell => cell.RowId == rowIds[1]).ExecuteDeleteAsync();
                await db.Rows.Where(row => row.Id == rowIds[1]).ExecuteDeleteAsync();

                Assert.NotNull(await edit.CommitAsync());

                // A step whose payload cannot be read, which is what a store written by a build that knew
                // a kind of step this one does not would look like.
                await db.Database.ExecuteSqlRawAsync("UPDATE TABLE_HISTORY SET PAYLOAD = 'not json';");
            }

            events.Clear();

            Assert.Null(await history.UndoAsync(path));

            // Nothing moved, so there is nothing to tell watchers about: the cursor stays where it was
            // rather than reporting a step as taken back that is still done.
            Assert.Empty(events);
            Assert.Equal(1, await history.GetUndoDepthAsync(path));
            Assert.Single((await ReadTableAsync(factory, path)).Rows);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Throwing_The_History_Away_Lets_The_Next_Step_START_AGAIN()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A"], [["a1"]]);

            await using var db = await factory.CreateDbContextAsync(path);

            for (var step = 1; step <= 2; step++)
            {
                await using var edit = history.BeginEdit(path, TableEditKind.RowsAdded, $"Added row {step}");

                var rowId = await TableStoreMaintenance.Create(db).AddRowAsync();

                await edit.CaptureAfterAsync(TableRegion.Rows([rowId]));

                Assert.NotNull(await edit.CommitAsync());
            }

            await history.ClearAsync(path);

            await using (var edit = history.BeginEdit(path, TableEditKind.RowsAdded, "Added row again"))
            {
                var rowId = await TableStoreMaintenance.Create(db).AddRowAsync();

                await edit.CaptureAfterAsync(TableRegion.Rows([rowId]));

                Assert.NotNull(await edit.CommitAsync());
            }

            var entry = Assert.Single(await history.GetEntriesAsync(path));

            // Throwing the history away forgets the numbering as well as the steps, so the history the
            // table starts again is numbered from the beginning.
            Assert.Equal(1, entry.Sequence);
            Assert.Equal("Added row again", entry.Description);

            // The step is a step of the new history, so it can be taken back and put in place again.
            Assert.True(await history.CanUndoAsync(path));
            Assert.NotNull(await history.UndoAsync(path));
            Assert.True(await history.CanRedoAsync(path));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task The_History_Is_Listed_Oldest_First_With_The_Cursor_Showing_What_Is_Done()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

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

            await history.UndoAsync(path);

            var entries = await history.GetEntriesAsync(path);

            Assert.Equal(
                ["Added row 1", "Added row 2", "Added row 3"],
                entries.Select(entry => entry.Description));

            // The step that was taken back is the newest one, and the ones before it are still done, so
            // the applied flags read from the far end of the list.
            Assert.Equal([true, true, false], entries.Select(entry => entry.IsApplied));
        }
        finally
        {
            DeleteStore(path);
        }
    }
}
