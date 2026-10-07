using Microsoft.Extensions.DependencyInjection;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.Extensions;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Interfaces;
using TableConverter.Utilities.Models;

namespace TableConverter.Utilities.Tests.Database;

/// <summary>
///     Covers boiling a table down to one row per group. What is asserted is the table that comes back,
///     rather than the running totals that produced it.
/// </summary>
public class TablePivotTests
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

    private static void AssertHeaders(IReadOnlyList<string> expected, IReadOnlyList<string> actual)
    {
        Assert.Equal(expected.Count, actual.Count);

        for (var index = 0; index < expected.Count; index++)
        {
            Assert.Equal(expected[index], actual[index]);
        }
    }

    private static void AssertRow(IReadOnlyList<string?> expected, IReadOnlyList<string?> actual)
    {
        Assert.Equal(expected.Count, actual.Count);

        for (var index = 0; index < expected.Count; index++)
        {
            Assert.Equal(expected[index], actual[index]);
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

    private static async Task<TableSnapshot> PivotAsync(
        ITableStoreDbContextFactory factory,
        string sourcePath,
        string destinationPath,
        IReadOnlyList<string> keyColumnNames,
        string? valueColumnName,
        AggregateKind aggregate)
    {
        await using (var sourceDb = await factory.CreateDbContextAsync(sourcePath))
        await using (var destinationDb = await factory.CreateDbContextAsync(destinationPath))
        {
            var source = TableStoreRowSource.Create(sourceDb);

            await using var sink = TableStoreRowSink.Create(destinationDb);

            await TablePivot.PivotAsync(source, sink, keyColumnNames, valueColumnName, aggregate);
        }

        var (headers, rows) = await ReadTableAsync(factory, destinationPath);

        return new TableSnapshot(headers, rows);
    }

    private static IReadOnlyList<TableColumn> SalesColumns()
    {
        return
        [
            new TableColumn("Region", ColumnDataType.Text),
            new TableColumn("Product", ColumnDataType.Text),
            new TableColumn("Amount", ColumnDataType.Integer),
        ];
    }

    private static Task WriteSalesAsync(ITableStoreDbContextFactory factory, string path)
    {
        return WriteThroughSinkAsync(
            factory,
            path,
            SalesColumns(),
            [
                ["North", "Widget", "10"],
                ["South", "Widget", "5"],
                ["North", "Widget", "20"],
                ["North", "Gadget", "7"],
            ]);
    }

    [Fact]
    public async Task GroupingByAColumnWritesOneRowPerGroup()
    {
        await using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var sourcePath = NewStorePath();
        var destinationPath = NewStorePath();

        try
        {
            await WriteSalesAsync(factory, sourcePath);

            var result = await PivotAsync(
                factory, sourcePath, destinationPath, ["Region"], "Amount", AggregateKind.Sum);

            AssertHeaders(["Region", "Sum of Amount"], result.Headers);
            AssertRow(["North", "37"], result.Rows[0]);
            AssertRow(["South", "5"], result.Rows[1]);
        }
        finally
        {
            DeleteStore(sourcePath);
            DeleteStore(destinationPath);
        }
    }

    [Fact]
    public async Task GroupsAreWrittenInTheOrderTheyAreFirstMet()
    {
        await using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var sourcePath = NewStorePath();
        var destinationPath = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory,
                sourcePath,
                SalesColumns(),
                [
                    ["South", "Widget", "5"],
                    ["North", "Widget", "10"],
                    ["South", "Gadget", "1"],
                    ["East", "Widget", "2"],
                ]);

            var result = await PivotAsync(
                factory, sourcePath, destinationPath, ["Region"], "Amount", AggregateKind.Sum);

            Assert.Equal("South", result.Rows[0][0]);
            Assert.Equal("North", result.Rows[1][0]);
            Assert.Equal("East", result.Rows[2][0]);
        }
        finally
        {
            DeleteStore(sourcePath);
            DeleteStore(destinationPath);
        }
    }

    [Fact]
    public async Task CountingWithoutAColumnCountsTheRowsOfEachGroup()
    {
        await using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var sourcePath = NewStorePath();
        var destinationPath = NewStorePath();

        try
        {
            await WriteSalesAsync(factory, sourcePath);

            var result = await PivotAsync(
                factory, sourcePath, destinationPath, ["Region"], null, AggregateKind.Count);

            AssertHeaders(["Region", "Count"], result.Headers);
            AssertRow(["North", "3"], result.Rows[0]);
            AssertRow(["South", "1"], result.Rows[1]);
        }
        finally
        {
            DeleteStore(sourcePath);
            DeleteStore(destinationPath);
        }
    }

    [Fact]
    public async Task CountingAColumnLeavesTheEmptyCellsOut()
    {
        await using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var sourcePath = NewStorePath();
        var destinationPath = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory,
                sourcePath,
                SalesColumns(),
                [
                    ["North", "Widget", "10"],
                    ["North", "Widget", null],
                    ["North", "Widget", "  "],
                ]);

            var result = await PivotAsync(
                factory, sourcePath, destinationPath, ["Region"], "Amount", AggregateKind.Count);

            AssertHeaders(["Region", "Count of Amount"], result.Headers);
            AssertRow(["North", "1"], result.Rows[0]);
        }
        finally
        {
            DeleteStore(sourcePath);
            DeleteStore(destinationPath);
        }
    }

    [Fact]
    public async Task AnAverageIsSharedOutOverTheValuesThereAreNotTheRows()
    {
        await using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var sourcePath = NewStorePath();
        var destinationPath = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory,
                sourcePath,
                SalesColumns(),
                [
                    ["North", "Widget", "10"],
                    ["North", "Widget", null],
                    ["North", "Widget", "20"],
                ]);

            var result = await PivotAsync(
                factory, sourcePath, destinationPath, ["Region"], "Amount", AggregateKind.Average);

            // Two values were read rather than three rows, so the average is 15 and not 10.
            AssertHeaders(["Region", "Average of Amount"], result.Headers);
            AssertRow(["North", "15"], result.Rows[0]);
        }
        finally
        {
            DeleteStore(sourcePath);
            DeleteStore(destinationPath);
        }
    }

    [Fact]
    public async Task TheSmallestAndTheLargestValueOfAGroupArePickedOut()
    {
        await using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var sourcePath = NewStorePath();
        var destinationPath = NewStorePath();

        try
        {
            await WriteSalesAsync(factory, sourcePath);

            var minimum = await PivotAsync(
                factory, sourcePath, destinationPath, ["Region"], "Amount", AggregateKind.Minimum);

            AssertHeaders(["Region", "Minimum of Amount"], minimum.Headers);
            AssertRow(["North", "7"], minimum.Rows[0]);

            var maximum = await PivotAsync(
                factory, sourcePath, destinationPath, ["Region"], "Amount", AggregateKind.Maximum);

            AssertHeaders(["Region", "Maximum of Amount"], maximum.Headers);
            AssertRow(["North", "20"], maximum.Rows[0]);
        }
        finally
        {
            DeleteStore(sourcePath);
            DeleteStore(destinationPath);
        }
    }

    [Fact]
    public async Task GroupingByTwoColumnsKeysEachRowByTheCombination()
    {
        await using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var sourcePath = NewStorePath();
        var destinationPath = NewStorePath();

        try
        {
            await WriteSalesAsync(factory, sourcePath);

            var result = await PivotAsync(
                factory, sourcePath, destinationPath, ["Region", "Product"], "Amount", AggregateKind.Sum);

            AssertHeaders(["Region", "Product", "Sum of Amount"], result.Headers);
            Assert.Equal(3, result.Rows.Count);
            AssertRow(["North", "Widget", "30"], result.Rows[0]);
            AssertRow(["South", "Widget", "5"], result.Rows[1]);
            AssertRow(["North", "Gadget", "7"], result.Rows[2]);
        }
        finally
        {
            DeleteStore(sourcePath);
            DeleteStore(destinationPath);
        }
    }

    [Fact]
    public async Task AValueThatDoesNotReadAsANumberIsLeftOutOfTheArithmetic()
    {
        await using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var sourcePath = NewStorePath();
        var destinationPath = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory,
                sourcePath,
                SalesColumns(),
                [
                    ["North", "Widget", "3"],
                    ["North", "Widget", "not a number"],
                    ["North", "Widget", "4"],
                ]);

            var result = await PivotAsync(
                factory, sourcePath, destinationPath, ["Region"], "Amount", AggregateKind.Sum);

            AssertRow(["North", "7"], result.Rows[0]);
        }
        finally
        {
            DeleteStore(sourcePath);
            DeleteStore(destinationPath);
        }
    }

    [Fact]
    public async Task AGroupWithNoReadableNumberSummarisesAsNothing()
    {
        await using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var sourcePath = NewStorePath();
        var destinationPath = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory,
                sourcePath,
                SalesColumns(),
                [
                    ["North", "Widget", "oops"],
                    ["North", "Widget", ""],
                ]);

            var result = await PivotAsync(
                factory, sourcePath, destinationPath, ["Region"], "Amount", AggregateKind.Sum);

            Assert.Equal("North", result.Rows[0][0]);
            Assert.Null(result.Rows[0][1]);
        }
        finally
        {
            DeleteStore(sourcePath);
            DeleteStore(destinationPath);
        }
    }

    [Fact]
    public async Task AWholeNumberColumnKeepsItsTypeAndAnAverageDoesNot()
    {
        await using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var sourcePath = NewStorePath();
        var destinationPath = NewStorePath();

        try
        {
            await WriteSalesAsync(factory, sourcePath);

            await using (var sourceDb = await factory.CreateDbContextAsync(sourcePath))
            await using (var destinationDb = await factory.CreateDbContextAsync(destinationPath))
            {
                var source = TableStoreRowSource.Create(sourceDb);

                await using var sink = TableStoreRowSink.Create(destinationDb);

                await TablePivot.PivotAsync(source, sink, ["Region"], "Amount", AggregateKind.Sum);
            }

            await using var readDb = await factory.CreateDbContextAsync(destinationPath);
            var columns = await TableStoreRowSource.Create(readDb).GetColumnsAsync();

            Assert.Equal(ColumnDataType.Integer, columns[^1].EffectiveDataType);
        }
        finally
        {
            DeleteStore(sourcePath);
            DeleteStore(destinationPath);
        }
    }

    [Fact]
    public async Task AnEmptyTableWritesTheColumnsAndNoRows()
    {
        await using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var sourcePath = NewStorePath();
        var destinationPath = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, sourcePath, SalesColumns(), []);

            var result = await PivotAsync(
                factory, sourcePath, destinationPath, ["Region"], "Amount", AggregateKind.Sum);

            AssertHeaders(["Region", "Sum of Amount"], result.Headers);
            Assert.Empty(result.Rows);
        }
        finally
        {
            DeleteStore(sourcePath);
            DeleteStore(destinationPath);
        }
    }

    [Fact]
    public async Task AskingForAColumnTheTableHasNotGotIsRefused()
    {
        await using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var sourcePath = NewStorePath();
        var destinationPath = NewStorePath();

        try
        {
            await WriteSalesAsync(factory, sourcePath);

            await using var sourceDb = await factory.CreateDbContextAsync(sourcePath);
            await using var destinationDb = await factory.CreateDbContextAsync(destinationPath);

            var source = TableStoreRowSource.Create(sourceDb);

            await using var sink = TableStoreRowSink.Create(destinationDb);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                TablePivot.PivotAsync(source, sink, ["Nowhere"], "Amount", AggregateKind.Sum));
        }
        finally
        {
            DeleteStore(sourcePath);
            DeleteStore(destinationPath);
        }
    }

    [Fact]
    public async Task SummarisingWithoutNamingAColumnIsRefusedUnlessTheRowsAreOnlyCounted()
    {
        await using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var sourcePath = NewStorePath();
        var destinationPath = NewStorePath();

        try
        {
            await WriteSalesAsync(factory, sourcePath);

            await using var sourceDb = await factory.CreateDbContextAsync(sourcePath);
            await using var destinationDb = await factory.CreateDbContextAsync(destinationPath);

            var source = TableStoreRowSource.Create(sourceDb);

            await using var sink = TableStoreRowSink.Create(destinationDb);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                TablePivot.PivotAsync(source, sink, ["Region"], null, AggregateKind.Sum));
        }
        finally
        {
            DeleteStore(sourcePath);
            DeleteStore(destinationPath);
        }
    }

    [Fact]
    public async Task SummarisingWithoutGroupingByAnythingIsRefused()
    {
        await using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();
        var sourcePath = NewStorePath();
        var destinationPath = NewStorePath();

        try
        {
            await WriteSalesAsync(factory, sourcePath);

            await using var sourceDb = await factory.CreateDbContextAsync(sourcePath);
            await using var destinationDb = await factory.CreateDbContextAsync(destinationPath);

            var source = TableStoreRowSource.Create(sourceDb);

            await using var sink = TableStoreRowSink.Create(destinationDb);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                TablePivot.PivotAsync(source, sink, [], "Amount", AggregateKind.Sum));
        }
        finally
        {
            DeleteStore(sourcePath);
            DeleteStore(destinationPath);
        }
    }

    [Theory]
    [InlineData(AggregateKind.Count, null, "Count")]
    [InlineData(AggregateKind.Count, "Amount", "Count of Amount")]
    [InlineData(AggregateKind.Sum, "Amount", "Sum of Amount")]
    [InlineData(AggregateKind.Average, "Amount", "Average of Amount")]
    [InlineData(AggregateKind.Minimum, "Amount", "Minimum of Amount")]
    [InlineData(AggregateKind.Maximum, "Amount", "Maximum of Amount")]
    public void TheResultColumnIsNamedForTheOperationAndTheColumn(
        AggregateKind aggregate, string? valueColumnName, string expected)
    {
        Assert.Equal(expected, TablePivot.Describe(aggregate, valueColumnName));
    }

    /// <summary>
    ///     The headers and rows a store held when it was read back.
    /// </summary>
    private sealed record TableSnapshot(IReadOnlyList<string> Headers, List<string?[]> Rows);
}

