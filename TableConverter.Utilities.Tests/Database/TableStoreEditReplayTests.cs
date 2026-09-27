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
/// Covers the records an entry is replayed from on their own, put in place and taken back against a store
/// directly. The end to end path goes through a recording; this exercises the writers underneath it,
/// including the row reorder that no operation in this build produces yet but that a sort will.
/// </summary>
public class TableStoreEditReplayTests
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

    [Fact]
    public async Task Reordering_Rows_Is_Replayed_And_Taken_Back()
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
                    ["a2", "b2"],
                    ["a3", "b3"],
                ]);

            var rowIds = await ReadRowIdsAsync(factory, path);

            // A turn of the rows: what was at the top is at the bottom and the other way round. The order
            // is the row ids, so putting the rows in a new order means renumbering them.
            var edit = new RowOrderEdit
            {
                Before = [.. rowIds],
                After = [rowIds[2], rowIds[1], rowIds[0]],
            };

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await edit.ApplyAsync(db);
            }

            var (_, applied) = await ReadTableAsync(factory, path);

            Assert.Equal(new string?[] { "a3", "b3" }, applied[0]);
            Assert.Equal(new string?[] { "a2", "b2" }, applied[1]);
            Assert.Equal(new string?[] { "a1", "b1" }, applied[2]);

            // The row ids are the order, so the values move with the rows into new ids rather than the
            // rows being reinserted with new values.
            Assert.Equal(rowIds, await ReadRowIdsAsync(factory, path));

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await edit.RevertAsync(db);
            }

            var (_, reverted) = await ReadTableAsync(factory, path);

            Assert.Equal(new string?[] { "a1", "b1" }, reverted[0]);
            Assert.Equal(new string?[] { "a2", "b2" }, reverted[1]);
            Assert.Equal(new string?[] { "a3", "b3" }, reverted[2]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Reordering_Rows_With_Orders_That_Do_Not_Match_Is_Rejected()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A"], [["a1"], ["a2"]]);

            var edit = new RowOrderEdit
            {
                Before = [1, 2],
                After = [1],
            };

            await using var db = await factory.CreateDbContextAsync(path);

            // An order that names a different number of rows than the order it replaces cannot describe a
            // reorder, and saying so is better than silently dropping a row.
            await Assert.ThrowsAsync<ArgumentException>(() => edit.ApplyAsync(db));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Composite_Is_Taken_Back_In_Reverse_Order()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A", "B"], [["a1", "b1"]]);

            var columns = await ReadColumnIdsAsync(factory, path);

            const int addedRowId = 10;

            // A row is added and then a value is written into it. Taking the two back in the order they
            // were taken would write into a row that is no longer there, so the composite has to undo the
            // last step first.
            var edit = new CompositeEdit
            {
                Edits =
                [
                    new RowsEdit
                    {
                        Removed = [],
                        Added =
                        [
                            new RowValues(addedRowId, new Dictionary<int, string?>
                            {
                                [columns["A"]] = "new",
                                [columns["B"]] = null,
                            }),
                        ],
                    },
                    new CellValuesEdit
                    {
                        Changes = [new CellChange(addedRowId, columns["A"], null, "changed")],
                    },
                ],
            };

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await edit.ApplyAsync(db);
            }

            var (_, applied) = await ReadTableAsync(factory, path);

            Assert.Equal(2, applied.Count);
            Assert.Equal(new string?[] { "a1", "b1" }, applied[0]);
            Assert.Equal(new string?[] { "changed", null }, applied[1]);
            Assert.Equal([1, addedRowId], await ReadRowIdsAsync(factory, path));

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await edit.RevertAsync(db);
            }

            var (_, reverted) = await ReadTableAsync(factory, path);

            Assert.Single(reverted);
            Assert.Equal(new string?[] { "a1", "b1" }, reverted[0]);
            Assert.Equal([1], await ReadRowIdsAsync(factory, path));

            await using var check = await factory.CreateDbContextAsync(path);

            // The value written into the added row must not be left behind as a cell of a row that is no
            // longer there.
            Assert.Equal(0, await check.Cells.AsNoTracking().CountAsync(cell => cell.RowId == addedRowId));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Whole_Table_Record_Replaces_What_Is_There_And_Puts_It_Back()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A"], [["a1"]]);

            var before = new TableSnapshot(
                [new ColumnValues(1, "A", ColumnDataType.Text, null, 1, null)],
                [new RowValues(1, new Dictionary<int, string?> { [1] = "a1" })]);

            var after = new TableSnapshot(
                [new ColumnValues(9, "Q", ColumnDataType.Text, null, 1, null)],
                [new RowValues(4, new Dictionary<int, string?> { [9] = "q1" })]);

            var edit = new TableReplaceEdit { Before = before, After = after };

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await edit.ApplyAsync(db);
            }

            var (appliedHeaders, appliedRows) = await ReadTableAsync(factory, path);

            Assert.Equal(["Q"], appliedHeaders);
            Assert.Equal(new string?[] { "q1" }, appliedRows[0]);
            Assert.Equal([4], await ReadRowIdsAsync(factory, path));

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                await edit.RevertAsync(db);
            }

            var (revertedHeaders, revertedRows) = await ReadTableAsync(factory, path);

            Assert.Equal(["A"], revertedHeaders);
            Assert.Equal(new string?[] { "a1" }, revertedRows[0]);

            // The snapshot holds the ids as well as the values, so a table put back is made of the rows
            // the grid already knows rather than of new ones that only look the same.
            Assert.Equal([1], await ReadRowIdsAsync(factory, path));
        }
        finally
        {
            DeleteStore(path);
        }
    }
}

