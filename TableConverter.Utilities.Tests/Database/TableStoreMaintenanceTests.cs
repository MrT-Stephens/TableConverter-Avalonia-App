using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.Extensions;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities.Tests.Database;

/// <summary>
/// Covers the table wide passes a user runs to clean a table up, plus the row insert the row utilities
/// use. These change a store in place with raw SQL, so what they leave behind is what is asserted
/// rather than the number of statements they ran.
/// </summary>
public class TableStoreMaintenanceTests
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

    private static async Task SetColumnDefaultAsync(
        ITableStoreDbContextFactory factory,
        string path,
        string columnName,
        string? defaultValue)
    {
        await using var dbContext = await factory.CreateDbContextAsync(path);

        var column = await dbContext.Columns.SingleAsync(entity => entity.Name == columnName);

        column.DefaultValueForCell = defaultValue;

        await dbContext.SaveChangesAsync();
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
    public async Task Trim_Removes_Padding_From_Cells_And_Column_Names()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

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

            await using var dbContext = await factory.CreateDbContextAsync(path);

            var maintenance = TableStoreMaintenance.Create(dbContext);

            // The two padded values and the value that is nothing but whitespace.
            Assert.Equal(3, await maintenance.TrimAsync());

            var (headers, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(["A", "B"], headers);
            Assert.Equal(["a1", "b1"], rows[0]);
            Assert.Equal(["a2", ""], rows[1]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Trim_Leaves_A_Name_That_Is_Only_Whitespace_Alone()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["   "], [["value"]]);

            await using var dbContext = await factory.CreateDbContextAsync(path);

            await TableStoreMaintenance.Create(dbContext).TrimAsync();

            var (headers, _) = await ReadTableAsync(factory, path);

            // Trimming it away would leave the column unnamed, which is worse than a name of spaces.
            Assert.Equal(["   "], headers);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Remove_Empty_Rows_Deletes_The_Rows_And_Their_Cells()
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
                    // Empty by an empty string, by whitespace and by a missing value.
                    ["", "  "],
                    [null, null],
                    ["a2", null],
                ]);

            await using var dbContext = await factory.CreateDbContextAsync(path);

            var maintenance = TableStoreMaintenance.Create(dbContext);

            Assert.Equal(2, await maintenance.RemoveEmptyRowsAsync());

            var (_, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(2, rows.Count);
            Assert.Equal("a1|b1", string.Join('|', rows[0]));
            Assert.Equal("a2|<null>", string.Join('|', rows[1].Select(value => value ?? "<null>")));

            // The cells of a deleted row have to go with it rather than being left orphaned. A row holds a
            // cell per column whether or not its value is missing, so the two rows that were kept are the
            // four cells that are left.
            await using var readContext = await factory.CreateDbContextAsync(path);

            Assert.Equal(4, await readContext.Cells.AsNoTracking().CountAsync());
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Remove_Duplicate_Rows_Keeps_The_First_Copy()
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
                    ["a", "b"],
                    ["a", "b"],
                    ["a", "c"],
                    ["a", "b"],
                    ["b", "a"],
                ]);

            await using var dbContext = await factory.CreateDbContextAsync(path);

            var maintenance = TableStoreMaintenance.Create(dbContext);

            Assert.Equal(2, await maintenance.RemoveDuplicateRowsAsync());

            var (_, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(3, rows.Count);
            Assert.Equal("a|b", string.Join('|', rows[0]));
            Assert.Equal("a|c", string.Join('|', rows[1]));
            Assert.Equal("b|a", string.Join('|', rows[2]));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Remove_Duplicate_Rows_Tells_Rows_Apart_By_Value_Boundaries()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            // Two rows whose values read the same once joined with a printable separator, which is why
            // the signature is joined with one that cannot appear in a value.
            await WriteThroughSinkAsync(
                factory,
                path,
                ["A", "B"],
                [
                    ["a|b", "c"],
                    ["a", "b|c"],
                ]);

            await using var dbContext = await factory.CreateDbContextAsync(path);

            Assert.Equal(0, await TableStoreMaintenance.Create(dbContext).RemoveDuplicateRowsAsync());

            var (_, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(2, rows.Count);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Remove_Duplicate_Rows_Ignores_A_Missing_Value()
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
                    ["a", null],
                    [null, null],
                    ["a", ""],
                ]);

            await using var dbContext = await factory.CreateDbContextAsync(path);

            // A missing value and an empty one both read as nothing, so the first and third rows are the
            // same row and the second one is not.
            Assert.Equal(1, await TableStoreMaintenance.Create(dbContext).RemoveDuplicateRowsAsync());

            var (_, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(2, rows.Count);
            Assert.Equal("a|<null>", string.Join('|', rows[0].Select(value => value ?? "<null>")));
            Assert.Equal("<null>|<null>", string.Join('|', rows[1].Select(value => value ?? "<null>")));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Remove_Empty_Columns_Deletes_Them_And_Renumbers_The_Rest()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(
                factory,
                path,
                ["A", "B", "C", "D"],
                [
                    ["a1", null, "", "d1"],
                    ["a2", null, null, "d2"],
                ]);

            await using var dbContext = await factory.CreateDbContextAsync(path);

            var maintenance = TableStoreMaintenance.Create(dbContext);

            Assert.Equal(2, await maintenance.RemoveEmptyColumnsAsync());

            var (headers, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(["A", "D"], headers);
            Assert.Equal(["a1", "d1"], rows[0]);
            Assert.Equal(["a2", "d2"], rows[1]);

            await using var readContext = await factory.CreateDbContextAsync(path);

            var columns = await readContext.Columns
                .AsNoTracking()
                .OrderBy(column => column.OrdinalPosition)
                .ToListAsync();

            // The grid's column order comes from the ordinal positions, so the gap the deleted columns
            // left has to be closed.
            Assert.Equal([1, 2], columns.Select(column => column.OrdinalPosition));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Remove_Empty_Columns_Keeps_A_Column_With_A_Value()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A", "B"], [["", "kept"]]);

            await using var dbContext = await factory.CreateDbContextAsync(path);

            Assert.Equal(1, await TableStoreMaintenance.Create(dbContext).RemoveEmptyColumnsAsync());

            var (headers, _) = await ReadTableAsync(factory, path);

            Assert.Equal(["B"], headers);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Fill_Empty_Cells_Uses_The_Default_Of_The_Column()
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
                    ["a1", null],
                    ["", "b2"],
                    ["   ", "b3"],
                ]);

            // Only one of the two columns has a default, so the cells of the other are left alone
            // rather than being filled with something the user never asked for.
            await SetColumnDefaultAsync(factory, path, "A", "unknown");

            await using var dbContext = await factory.CreateDbContextAsync(path);

            var maintenance = TableStoreMaintenance.Create(dbContext);

            Assert.Equal(2, await maintenance.FillEmptyCellsAsync());

            var (_, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(["a1", null], rows[0]);
            Assert.Equal(["unknown", "b2"], rows[1]);
            Assert.Equal(["unknown", "b3"], rows[2]);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Add_Row_Appends_A_Row_Of_COLUMN_DEFAULTS()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, ["A", "B"], [["a1", "b1"]]);

            await SetColumnDefaultAsync(factory, path, "A", "n/a");

            await using var dbContext = await factory.CreateDbContextAsync(path);

            var rowId = await TableStoreMaintenance.Create(dbContext).AddRowAsync();

            Assert.True(rowId > 0);

            var (_, rows) = await ReadTableAsync(factory, path);

            Assert.Equal(2, rows.Count);
            Assert.Equal("n/a|<null>", string.Join('|', rows[1].Select(value => value ?? "<null>")));
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Add_Row_Appends_A_Row_To_A_Table_With_No_Columns()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            await WriteThroughSinkAsync(factory, path, [], []);

            await using var dbContext = await factory.CreateDbContextAsync(path);

            var rowId = await TableStoreMaintenance.Create(dbContext).AddRowAsync();

            await using var readContext = await factory.CreateDbContextAsync(path);

            Assert.Equal(1, await readContext.Rows.AsNoTracking().CountAsync());
            Assert.Equal(rowId, await readContext.Rows.AsNoTracking().Select(row => row.Id).SingleAsync());
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Statistics_Summarise_Each_Column()
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
                    ["x", "1"],
                    ["x", "2"],
                    ["", null],
                ]);

            await using var dbContext = await factory.CreateDbContextAsync(path);

            var statistics = await TableStoreStatistics.ReadAsync(dbContext);

            Assert.Equal(3, statistics.RowCount);
            Assert.Equal(2, statistics.ColumnCount);
            Assert.Equal(4, statistics.FilledCellCount);

            // A row that holds nothing for a column counts as empty whether its cell is missing or blank.
            Assert.Equal(2, statistics.EmptyCellCount);

            var first = statistics.Columns[0];

            Assert.Equal("A", first.Name);
            Assert.Equal(1, first.OrdinalPosition);
            Assert.Equal(2, first.FilledCount);
            Assert.Equal(1, first.EmptyCount);
            Assert.Equal(1, first.DistinctCount);

            var second = statistics.Columns[1];

            Assert.Equal("B", second.Name);
            Assert.Equal(2, second.OrdinalPosition);
            Assert.Equal(2, second.FilledCount);
            Assert.Equal(1, second.EmptyCount);
            Assert.Equal(2, second.DistinctCount);
        }
        finally
        {
            DeleteStore(path);
        }
    }
}
