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
/// Covers taking every kind of recorded step back and putting it in place again, region by region: the
/// cells a grid editor rewrites, the rows a clean up pass removes, the columns a tidy up drops, and the
/// whole table an import replaces. What is asserted is the table the store is left holding.
/// </summary>
public class TableStoreHistoryReplayTests
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

    private static async Task<List<int>> ReadRowIdsAsync(ITableStoreDbContextFactory factory, string path)
    {
        await using var dbContext = await factory.CreateDbContextAsync(path);

        return await dbContext.Rows
            .AsNoTracking()
            .OrderBy(row => row.Id)
            .Select(row => row.Id)
            .ToListAsync();
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

    private static async Task<List<(string Name, int OrdinalPosition)>> ReadColumnOrderAsync(
        ITableStoreDbContextFactory factory,
        string path)
    {
        await using var dbContext = await factory.CreateDbContextAsync(path);

        var columns = await dbContext.Columns
            .AsNoTracking()
            .OrderBy(column => column.OrdinalPosition)
            .Select(column => new { column.Name, column.OrdinalPosition })
            .ToListAsync();

        return [.. columns.Select(column => (column.Name, column.OrdinalPosition))];
    }

    /// <summary>
    /// Rewrites one cell the way a grid editor does, through the change tracker of its own short lived
    /// context.
    /// </summary>
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

    private static async Task RenameColumnAsync(
        ITableStoreDbContextFactory factory,
        string path,
        int columnId,
        string name)
    {
        await using var dbContext = await factory.CreateDbContextAsync(path);

        var column = await dbContext.Columns.SingleAsync(column => column.Id == columnId);

        column.Name = name;

        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Removes a column, and the values under it, with statements the way a column drop does.
    /// </summary>
    private static async Task RemoveColumnAsync(
        ITableStoreDbContextFactory factory,
        string path,
        int columnId)
    {
        await using var dbContext = await factory.CreateDbContextAsync(path);

        await dbContext.Cells.Where(cell => cell.ColumnId == columnId).ExecuteDeleteAsync();
        await dbContext.Columns.Where(column => column.Id == columnId).ExecuteDeleteAsync();
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

    private static async Task SetColumnDefaultAsync(
        ITableStoreDbContextFactory factory,
        string path,
        string columnName,
        string? defaultValue)
    {
        await using var dbContext = await factory.CreateDbContextAsync(path);

        var column = await dbContext.Columns.SingleAsync(column => column.Name == columnName);

        column.DefaultValueForCell = defaultValue;

        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task A_Changed_Cell_Is_Undone_And_Put_Back()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A", "B"], [["a1", "b1"], ["a2", "b2"]]);

            var columns = await ReadColumnIdsAsync(factory, path);
            var rowIds = await ReadRowIdsAsync(factory, path);

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await using var edit = history.BeginEdit(path, TableEditKind.CellsChanged, "Changed a cell");

                // The cell editor names the one cell it is about to write rather than the row around it.
                await edit.CaptureBeforeAsync(TableRegion.Cells([(rowIds[0], columns["A"])]));

                await SetCellAsync(factory, path, rowIds[0], columns["A"], "a1x");

                var entry = await edit.CommitAsync();

                Assert.NotNull(entry);
                Assert.Equal(TableEditKind.CellsChanged, entry.Kind);
            }

            var (_, changed) = await ReadTableAsync(factory, path);

            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a1x", "b1" }, changed[0]);
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a2", "b2" }, changed[1]);

            await history.UndoAsync(path);

            var (_, restored) = await ReadTableAsync(factory, path);

            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a1", "b1" }, restored[0]);
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a2", "b2" }, restored[1]);

            await history.RedoAsync(path);

            var (_, changedAgain) = await ReadTableAsync(factory, path);

            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a1x", "b1" }, changedAgain[0]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Several_Cells_Recorded_As_One_Step_Are_Undone_Together()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A", "B"], [["a1", "b1"], ["a2", "b2"]]);

            var columns = await ReadColumnIdsAsync(factory, path);
            var rowIds = await ReadRowIdsAsync(factory, path);

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await using var edit = history.BeginEdit(path, TableEditKind.CellsChanged, "Changed cells");

                await edit.CaptureBeforeAsync(TableRegion.Cells(
                [
                    (rowIds[0], columns["A"]),
                    (rowIds[1], columns["B"]),
                ]));

                await SetCellAsync(factory, path, rowIds[0], columns["A"], "a1x");
                await SetCellAsync(factory, path, rowIds[1], columns["B"], "b2x");

                Assert.NotNull(await edit.CommitAsync());
            }

            var (_, changed) = await ReadTableAsync(factory, path);

            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a1x", "b1" }, changed[0]);
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a2", "b2x" }, changed[1]);

            // One step the user took is one step they take back, even though it touched two cells in two
            // different rows.
            await history.UndoAsync(path);

            var (_, restored) = await ReadTableAsync(factory, path);

            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a1", "b1" }, restored[0]);
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a2", "b2" }, restored[1]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Value_Cleared_To_Nothing_Comes_Back()
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

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await using var edit = history.BeginEdit(path, TableEditKind.CellsChanged, "Cleared a cell");

                await edit.CaptureBeforeAsync(TableRegion.Cells([(rowIds[0], columns["A"])]));

                await SetCellAsync(factory, path, rowIds[0], columns["A"], null);

                Assert.NotNull(await edit.CommitAsync());
            }

            var (_, cleared) = await ReadTableAsync(factory, path);

            Assert.Null(cleared[0][0]);

            await history.UndoAsync(path);

            var (_, restored) = await ReadTableAsync(factory, path);

            Assert.Equal("a1", restored[0][0]);

            await history.RedoAsync(path);

            Assert.Null((await ReadTableAsync(factory, path)).Rows[0][0]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Value_Put_Into_A_Cell_That_Held_Nothing_Is_Undone()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A", "B"], [[null, "b1"]]);

            var columns = await ReadColumnIdsAsync(factory, path);
            var rowIds = await ReadRowIdsAsync(factory, path);

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await using var edit = history.BeginEdit(path, TableEditKind.CellsChanged, "Filled a cell");

                await edit.CaptureBeforeAsync(TableRegion.Cells([(rowIds[0], columns["A"])]));

                await SetCellAsync(factory, path, rowIds[0], columns["A"], "a1");

                Assert.NotNull(await edit.CommitAsync());
            }

            Assert.Equal("a1", (await ReadTableAsync(factory, path)).Rows[0][0]);

            await history.UndoAsync(path);

            // A value that was not there is put back as not there, rather than as a value that reads the
            // same, so a pass that treats a missing value as a missing one still sees it as missing.
            Assert.Null((await ReadTableAsync(factory, path)).Rows[0][0]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Cell_Changed_Through_A_Row_Region_Is_Undone()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A", "B"], [["a1", "b1"]]);

            var columns = await ReadColumnIdsAsync(factory, path);
            var rowIds = await ReadRowIdsAsync(factory, path);

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await using var edit = history.BeginEdit(path, TableEditKind.CellsChanged, "Changed cells");

                // A pass that rewrites a whole row names the row rather than each cell, so the values it
                // changes are worked out from the difference between the two readings of the row.
                await edit.CaptureBeforeAsync(TableRegion.Rows([rowIds[0]]));

                await SetCellAsync(factory, path, rowIds[0], columns["A"], "a1x");
                await SetCellAsync(factory, path, rowIds[0], columns["B"], "b1x");

                Assert.NotNull(await edit.CommitAsync());
            }

            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a1x", "b1x" }, (await ReadTableAsync(factory, path)).Rows[0]);

            await history.UndoAsync(path);

            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a1", "b1" }, (await ReadTableAsync(factory, path)).Rows[0]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Removing_A_Column_Is_Undone_Without_Losing_The_Other_Columns()
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
                ["A", "B", "C"],
                [
                    ["a1", "b1", "c1"],
                    ["a2", "b2", "c2"],
                ]);

            var columns = await ReadColumnIdsAsync(factory, path);

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await using var edit = history.BeginEdit(path, TableEditKind.ColumnsChanged, "Removed column B");

                // Only the column being removed needs its values remembered; the columns around it keep
                // theirs, which is what the undo has to leave alone.
                await edit.CaptureBeforeAsync(TableRegion.Columns([columns["B"]]));

                await RemoveColumnAsync(factory, path, columns["B"]);

                Assert.NotNull(await edit.CommitAsync());
            }

            var (removedHeaders, removedRows) = await ReadTableAsync(factory, path);

            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "A", "C" }, removedHeaders);
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a1", "c1" }, removedRows[0]);

            await history.UndoAsync(path);

            var (headers, rows) = await ReadTableAsync(factory, path);

            // This is the whole point of remembering the removed column: it comes back with its values,
            // in the place it was, and the columns on either side are exactly as they were.
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "A", "B", "C" }, headers);
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a1", "b1", "c1" }, rows[0]);
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a2", "b2", "c2" }, rows[1]);

            await history.RedoAsync(path);

            var (removedAgain, rowsAgain) = await ReadTableAsync(factory, path);

            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "A", "C" }, removedAgain);

            // Putting the removal back in place must not run off with the values of the columns that stay.
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a1", "c1" }, rowsAgain[0]);
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a2", "c2" }, rowsAgain[1]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Renaming_A_Column_Is_Undone_Without_Remembering_Its_Values()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A", "B"], [["a1", "b1"]]);

            var columns = await ReadColumnIdsAsync(factory, path);

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await using var edit = history.BeginEdit(path, TableEditKind.ColumnsChanged, "Renamed column A");

                // A rename needs none of the values under the column, which is what keeps a rename of a
                // wide table from costing as much as the table.
                await edit.CaptureBeforeAsync(TableRegion.Columns());

                await RenameColumnAsync(factory, path, columns["A"], "Z");

                Assert.NotNull(await edit.CommitAsync());
            }

            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "Z", "B" }, (await ReadTableAsync(factory, path)).Headers);

            await history.UndoAsync(path);

            var (headers, rows) = await ReadTableAsync(factory, path);

            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "A", "B" }, headers);

            // Nothing remembered the values because nothing had to, and they are not disturbed by the
            // name going back.
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a1", "b1" }, rows[0]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Removing_Empty_Columns_Is_Undone_With_Their_Positions()
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
                ["A", "B", "C"],
                [
                    ["a1", null, "c1"],
                    ["a2", "", "c2"],
                ]);

            var columns = await ReadColumnIdsAsync(factory, path);

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                var maintenance = TableStoreMaintenance.Create(db);

                await using var edit = history.BeginEdit(path, TableEditKind.ColumnsChanged, "Removed empty columns");

                await edit.CaptureBeforeAsync(
                    TableRegion.Columns(await maintenance.GetEmptyColumnIdsAsync()));

                Assert.Equal(1, await maintenance.RemoveEmptyColumnsAsync());

                Assert.NotNull(await edit.CommitAsync());
            }

            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "A", "C" }, (await ReadTableAsync(factory, path)).Headers);

            await history.UndoAsync(path);

            var (headers, _) = await ReadTableAsync(factory, path);

            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "A", "B", "C" }, headers);

            // Removing a column closes the gap it left, so putting it back has to reopen the gap as well
            // as restore the column itself.
            Assert.Equal(
                [("A", 1), ("B", 2), ("C", 3)],
                await ReadColumnOrderAsync(factory, path));

            await history.RedoAsync(path);

            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "A", "C" }, (await ReadTableAsync(factory, path)).Headers);
            Assert.Equal([("A", 1), ("C", 2)], await ReadColumnOrderAsync(factory, path));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Filling_Empty_Cells_Is_Undone_With_The_Values_That_Were_There()
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
                    ["a1", null],
                    ["", "b2"],
                ]);

            await SetColumnDefaultAsync(factory, path, "A", "unknown");
            await SetColumnDefaultAsync(factory, path, "B", "n/a");

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                var maintenance = TableStoreMaintenance.Create(db);

                await using var edit = history.BeginEdit(path, TableEditKind.CellsChanged, "Filled empty cells");

                await edit.CaptureBeforeAsync(
                    TableRegion.Rows(await maintenance.GetRowIdsToFillAsync()));

                Assert.Equal(2, await maintenance.FillEmptyCellsAsync());

                Assert.NotNull(await edit.CommitAsync());
            }

            var (_, filled) = await ReadTableAsync(factory, path);

            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a1", "n/a" }, filled[0]);
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "unknown", "b2" }, filled[1]);

            await history.UndoAsync(path);

            var (_, restored) = await ReadTableAsync(factory, path);

            // The difference between nothing and nothing-but-whitespace is kept, because a fill treats
            // both as empty but is taken back to what each actually held.
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a1", null }, restored[0]);
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "", "b2" }, restored[1]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Removing_And_Adding_Rows_In_One_Step_Is_Undone()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A", "B"], [["a1", "b1"], ["a2", "b2"]]);

            var columns = await ReadColumnIdsAsync(factory, path);
            var rowIds = await ReadRowIdsAsync(factory, path);

            TableEditKind committedKind;

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                // The kind is left unknown so the entry names itself after what it turned out to hold,
                // which for a step that does three things is the composite.
                await using var edit = history.BeginEdit(path, TableEditKind.Unknown, "Replaced a row");

                await edit.CaptureBeforeAsync(TableRegion.Rows(rowIds));

                await RemoveRowsAsync(factory, path, [rowIds[1]]);
                await SetCellAsync(factory, path, rowIds[0], columns["A"], "a1x");

                var addedRowId = await TableStoreMaintenance.Create(db).AddRowAsync();

                await edit.CaptureAfterAsync(TableRegion.Rows([addedRowId]));

                var entry = await edit.CommitAsync();

                Assert.NotNull(entry);
                committedKind = entry.Kind;
            }

            Assert.Equal(TableEditKind.Composite, committedKind);

            var (_, replaced) = await ReadTableAsync(factory, path);

            Assert.Equal(2, replaced.Count);
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a1x", "b1" }, replaced[0]);
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { null, null }, replaced[1]);

            await history.UndoAsync(path);

            var (_, restored) = await ReadTableAsync(factory, path);

            // The removal, the value change and the addition are all taken back, and both rows come back
            // under the ids they held so the grid that is showing them still recognises them.
            Assert.Equal(2, restored.Count);
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a1", "b1" }, restored[0]);
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a2", "b2" }, restored[1]);
            Assert.Equal(rowIds, await ReadRowIdsAsync(factory, path));

            await history.RedoAsync(path);

            var (_, replacedAgain) = await ReadTableAsync(factory, path);

            Assert.Equal(2, replacedAgain.Count);
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a1x", "b1" }, replacedAgain[0]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Several_Steps_Are_Undone_In_Order_And_Put_Back_In_Order()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A"], [["a1"], ["a2"], ["a3"]]);

            var columns = await ReadColumnIdsAsync(factory, path);
            var rowIds = await ReadRowIdsAsync(factory, path);

            // Step one changes a value, step two removes a row, step three adds a row.
            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await using (var edit = history.BeginEdit(path, TableEditKind.CellsChanged, "Changed a value"))
                {
                    await edit.CaptureBeforeAsync(TableRegion.Cells([(rowIds[0], columns["A"])]));
                    await SetCellAsync(factory, path, rowIds[0], columns["A"], "x1");
                    Assert.NotNull(await edit.CommitAsync());
                }

                await using (var edit = history.BeginEdit(path, TableEditKind.RowsDeleted, "Removed a row"))
                {
                    await edit.CaptureBeforeAsync(TableRegion.Rows([rowIds[1]]));
                    await RemoveRowsAsync(factory, path, [rowIds[1]]);
                    Assert.NotNull(await edit.CommitAsync());
                }

                await using (var edit = history.BeginEdit(path, TableEditKind.RowsAdded, "Added a row"))
                {
                    var addedRowId = await TableStoreMaintenance.Create(db).AddRowAsync();
                    await edit.CaptureAfterAsync(TableRegion.Rows([addedRowId]));
                    Assert.NotNull(await edit.CommitAsync());
                }
            }

            Assert.Equal(3, await history.GetUndoDepthAsync(path));
            Assert.Equal(0, await history.GetRedoDepthAsync(path));

            var (_, afterSteps) = await ReadTableAsync(factory, path);

            Assert.Equal(3, afterSteps.Count);
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "x1" }, afterSteps[0]);
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a3" }, afterSteps[1]);

            // Taking the steps back one at a time walks the table back to where it started.
            var undone = await history.UndoAsync(path);

            Assert.Equal("Added a row", undone?.Description);
            Assert.Equal(2, (await ReadTableAsync(factory, path)).Rows.Count);

            undone = await history.UndoAsync(path);

            Assert.Equal("Removed a row", undone?.Description);
            Assert.Equal(3, (await ReadTableAsync(factory, path)).Rows.Count);

            undone = await history.UndoAsync(path);

            Assert.Equal("Changed a value", undone?.Description);
            Assert.Equal(0, await history.GetUndoDepthAsync(path));
            Assert.Equal(3, await history.GetRedoDepthAsync(path));

            var (_, walkedBack) = await ReadTableAsync(factory, path);

            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a1" }, walkedBack[0]);
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a2" }, walkedBack[1]);
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a3" }, walkedBack[2]);
            Assert.Equal(rowIds, await ReadRowIdsAsync(factory, path));

            Assert.Null(await history.UndoAsync(path));

            // Putting them back one at a time walks the table forward to where it had got to.
            Assert.Equal("Changed a value", (await history.RedoAsync(path))?.Description);
            Assert.Equal("Removed a row", (await history.RedoAsync(path))?.Description);
            Assert.Equal("Added a row", (await history.RedoAsync(path))?.Description);

            Assert.Equal(3, await history.GetUndoDepthAsync(path));
            Assert.Equal(0, await history.GetRedoDepthAsync(path));
            Assert.Null(await history.RedoAsync(path));

            var (_, walkedForward) = await ReadTableAsync(factory, path);

            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "x1" }, walkedForward[0]);
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a3" }, walkedForward[1]);
            Assert.Equal(3, walkedForward.Count);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task An_Import_Over_An_Existing_Table_Is_Undone_From_Its_Snapshot()
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
                await using var edit = history.BeginEdit(path, TableEditKind.TableReplaced, "Imported a table");

                await edit.CaptureBeforeAsync(TableRegion.Table());

                // An import clears the store and writes the table it parsed, which is a replacement of the
                // whole table rather than a change to a part of it.
                await WriteThroughSinkAsync(factory, path, ["B", "C"], [["b1", "c1"]]);

                var entry = await edit.CommitAsync();

                Assert.NotNull(entry);
                Assert.Equal(TableEditKind.TableReplaced, entry.Kind);
            }

            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "B", "C" }, (await ReadTableAsync(factory, path)).Headers);

            await history.UndoAsync(path);

            var (headers, rows) = await ReadTableAsync(factory, path);

            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "A" }, headers);
            Assert.Equal(2, rows.Count);
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a1" }, rows[0]);
            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "a2" }, rows[1]);

            // The table comes back under the ids it had, so a grid still showing it does not lose its
            // place to a restored table that looks the same but is made of different rows.
            Assert.Equal(rowIds, await ReadRowIdsAsync(factory, path));

            await history.RedoAsync(path);

            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "B", "C" }, (await ReadTableAsync(factory, path)).Headers);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Filling_An_Empty_Table_Is_Not_Recorded_As_A_Step()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var history = provider.GetRequiredService<ITableHistory>();

        var path = NewStorePath();

        try
        {
            // A store that holds nothing yet.
            await WriteThroughSinkAsync(factory, path, [], []);

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await using var edit = history.BeginEdit(path, TableEditKind.TableReplaced, "Imported a table");

                await edit.CaptureBeforeAsync(TableRegion.Table());

                await WriteThroughSinkAsync(factory, path, ["A"], [["a1"], ["a2"]]);

                // Filling a table that held nothing is the one step that is never kept: it would double
                // the size of a freshly imported store to buy a step nobody wants.
                Assert.Null(await edit.CommitAsync());
            }

            Assert.Empty(await history.GetEntriesAsync(path));
            Assert.False(await history.CanUndoAsync(path));

            var (headers, rows) = await ReadTableAsync(factory, path);

            Assert.Equal<IReadOnlyList<string?>>(new string?[] { "A" }, headers);
            Assert.Equal(2, rows.Count);
        }
        finally
        {
            DeleteStore(path);
        }
    }
}

