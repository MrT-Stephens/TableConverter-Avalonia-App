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

namespace TableConverter.Tests.Columns;

/// <summary>
///     Checks that the values a row shows sit under the headings their columns hold, once a column has
///     been moved along the table.
/// </summary>
/// <remarks>
///     A cell is shown by the place its column holds rather than by the cell's own id, so a move - which
///     changes a column's place but not its id - has to move the values with the column. This is read
///     through the rows' own data source, which is what the table's grid is built on.
/// </remarks>
public class ColumnMoveRowDataTests
{
    /// <summary>
    ///     Runs the actions ModelFlow would hand to the UI thread where they are raised. A data source cannot
    ///     be built without a hook, and a test host has no dispatcher to hand them to.
    /// </summary>
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

    [Fact]
    public async Task A_Rows_Cells_Are_Ordered_By_The_Place_Their_Column_Holds()
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
                ThreeTextColumns(),
                [["a1", "b1", "c1"], ["a2", "b2", "c2"]]);

            var columns = new TableStoreColumnsDataSource(factory, history) { Path = path };

            var columnA = await FindColumnAsync(columns, "A");

            Assert.NotNull(columnA);

            // Moving A one place towards the end leaves B holding the first place and A the second.
            Assert.True(await columns.MoveAsync(columnA, 1));

            List<int> columnIdsByOrdinal;

            await using (var db = await factory.CreateDbContextAsync(path))
            {
                columnIdsByOrdinal = await db.Columns
                    .AsNoTracking()
                    .OrderBy(column => column.OrdinalPosition)
                    .Select(column => column.Id)
                    .ToListAsync();
            }

            var rows = new TableStoreDataSource(factory, history) { Path = path };

            // Reading a page of models is the read the grid's provider makes to fill a row, and it is the
            // read the cells are put in order in, so the test goes through it rather than through a
            // placeholder the grid has not asked for yet.
            var row = (await rows.GetModelsAtAsync(0, 1)).First();

            // The first cell the grid reads has to be the cell of the column holding the first place, and
            // so on along the row. Reading the row by index is what the grid's columns are bound to, so its
            // order is the whole of what decides which value appears under which heading.
            string?[] movedValues = ["b1", "a1", "c1"];

            for (var index = 0; index < columnIdsByOrdinal.Count; index++)
            {
                Assert.Equal(movedValues[index], row.Cells[index].Value);
                Assert.Equal(columnIdsByOrdinal[index], row.Cells[index].ColumnId);
            }
        }
        finally
        {
            DeleteStore(path);
        }
    }
}

