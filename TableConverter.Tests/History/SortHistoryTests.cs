using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Commands.Handlers.TableData;
using TableConverter.Utilities;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.Extensions;
using TableConverter.Utilities.Database.History;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Interfaces;
using TableConverter.Utilities.Models;

namespace TableConverter.Tests.History;

/// <summary>
///     Runs the real statement the sort is carried out with, rather than a stand-in for it, and puts it
///     through a history step the way the command does. The rows of a freshly written table are numbered
///     from one, which is the same range the sort renumbers them to, so a step that remembered the row ids
///     alone would see no change at all and leave the sort unable to be taken back.
/// </summary>
/// <remarks>
///     The statement is read off the handler by reflection because it is the handler's own, and the point of
///     the test is that what the application runs is what is checked. A rename of the field shows up as a
///     failure here rather than as a silently skipped check.
/// </remarks>
public class SortHistoryTests
{
    private static readonly string SortSql = (string)typeof(SortByColumnCommandHandler)
        .GetField("SortSql", BindingFlags.NonPublic | BindingFlags.Static)!
        .GetValue(null)!;

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

    /// <summary>
    ///     Runs the sort the way the command does, inside a step that remembers the whole table first.
    /// </summary>
    private static async Task<HistoryEntry?> SortAsync(
        ITableStoreDbContextFactory factory,
        ITableHistory history,
        string path,
        string columnName,
        bool readAsNumber,
        bool descending)
    {
        await using var db = await factory.CreateDbContextAsync(path);

        var column = await db.Columns
            .AsNoTracking()
            .OrderBy(candidate => candidate.OrdinalPosition)
            .FirstAsync(candidate => candidate.Name == columnName);

        await using var edit = history.BeginEdit(
            path,
            TableEditKind.RowOrderChanged,
            $"Sorted by '{column.Name}' {(descending ? "descending" : "ascending")}");

        // An ordering of the ids cannot describe the move, because the ids are the ordering: what the sort
        // does is move values between rows, so the values are what has to be remembered.
        await edit.CaptureBeforeAsync(TableRegion.Table());

        await db.Database.ExecuteSqlRawAsync(
            SortSql,
            new SqliteParameter("@COLUMN_ID", column.Id),
            new SqliteParameter("@READ_AS_NUMBER", readAsNumber ? 1 : 0),
            new SqliteParameter("@DESCENDING", descending ? 1 : 0));

        return await edit.CommitAsync();
    }

    [Fact]
    public async Task Sorting_A_Numeric_Column_Reads_Its_Values_As_Numbers()
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
                ["Name", "Amount"],
                [["b", "10"], ["a", "2"], ["c", "100"]]);

            var entry = await SortAsync(factory, history, path, "Amount", readAsNumber: true, descending: false);

            Assert.NotNull(entry);

            var (headers, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(["Name", "Amount"], headers);

            // Read as text 10 would come before 2; read as the numbers they are, they come in number order.
            Assert.Equal(["a", "b", "c"], rows.Select(row => row[0]));
            Assert.Equal(["2", "10", "100"], rows.Select(row => row[1]));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Sort_Of_Contiguous_Ids_Is_Recorded_And_Taken_Back()
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
                ["Name", "Amount"],
                [["b", "10"], ["a", "2"], ["c", "100"]]);

            var entry = await SortAsync(factory, history, path, "Amount", readAsNumber: true, descending: false);

            // The rows keep the ids 1, 2 and 3 they were written with, so the step is one that has to be
            // recorded even though the set of ids it left behind is the one it started with.
            Assert.NotNull(entry);
            Assert.Equal(TableEditKind.RowOrderChanged, entry.Kind);
            Assert.Equal("Sorted by 'Amount' ascending", entry.Description);

            var (sortedHeaders, sortedRows) = await ReadTableAsync(factory, path);

            Assert.Equal(["Name", "Amount"], sortedHeaders);
            Assert.Equal(["a", "b", "c"], sortedRows.Select(row => row[0]));

            var undone = await history.UndoAsync(path);

            Assert.NotNull(undone);

            // Taking the step back puts the values back under the ids they were written with, so both what
            // each row holds and the order the rows are in return to what they were.
            var (undoneHeaders, undoneRows) = await ReadTableAsync(factory, path);

            Assert.Equal(["Name", "Amount"], undoneHeaders);
            Assert.Equal(["b", "a", "c"], undoneRows.Select(row => row[0]));
            Assert.Equal(["10", "2", "100"], undoneRows.Select(row => row[1]));

            await history.RedoAsync(path);

            var redoneRows = await ReadTableAsync(factory, path);

            Assert.Equal(["a", "b", "c"], redoneRows.Rows.Select(row => row[0]));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Descending_Sort_Is_Taken_Back_To_The_Order_It_Found()
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
                ["Name", "Amount"],
                [["b", "10"], ["a", "2"], ["c", "100"]]);

            await SortAsync(factory, history, path, "Amount", readAsNumber: true, descending: true);

            var descendingRows = await ReadTableAsync(factory, path);

            Assert.Equal(["c", "b", "a"], descendingRows.Rows.Select(row => row[0]));

            await history.UndoAsync(path);

            var undoneRows = await ReadTableAsync(factory, path);

            Assert.Equal(["b", "a", "c"], undoneRows.Rows.Select(row => row[0]));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Missing_Value_Sorts_Alongside_The_Empty_Text_It_Is_Shown_As()
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
                ["Name"],
                [["b"], [null], ["a"]]);

            await SortAsync(factory, history, path, "Name", readAsNumber: false, descending: false);

            var (headers, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(["Name"], headers);

            // The row with nothing in it is read as the empty text it is shown as, which comes before a name.
            Assert.Equal(new List<string?> { null, "a", "b" }, rows.Select(row => row[0]));

            await history.UndoAsync(path);

            var undoneRows = await ReadTableAsync(factory, path);

            Assert.Equal(["b", null, "a"], undoneRows.Rows.Select(row => row[0]));
        }
        finally
        {
            DeleteStore(path);
        }
    }
}
