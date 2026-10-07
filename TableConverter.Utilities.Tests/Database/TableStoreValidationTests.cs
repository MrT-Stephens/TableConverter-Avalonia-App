using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.Extensions;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Interfaces;
using TableConverter.Utilities.Models;

namespace TableConverter.Utilities.Tests.Database;

/// <summary>
///     Covers the read that says which of a table's values do not read as the type of the column they are
///     under. A column's type is metadata rather than a constraint, so this read is what turns it into an
///     answer the validation panel can show.
/// </summary>
public class TableStoreValidationTests
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
    ///     Writes a table whose columns name their own types, exactly the way an importer does.
    /// </summary>
    private static async Task WriteTypedThroughSinkAsync(
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

    private static async Task<TableValidationReport> ValidateAsync(
        ITableStoreDbContextFactory factory,
        string path,
        int maximumReportedCells = TableStoreValidation.DefaultMaximumReportedCells)
    {
        await using var dbContext = await factory.CreateDbContextAsync(path);

        return await TableStoreValidation.ReadAsync(dbContext, maximumReportedCells);
    }

    [Fact]
    public async Task A_Value_That_Does_Not_Read_As_Its_Column_Type_Is_Reported_Where_It_Sits()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            await WriteTypedThroughSinkAsync(
                factory,
                path,
                [new TableColumn("Age", ColumnDataType.Integer), new TableColumn("Name", ColumnDataType.Text)],
                [
                    ["30", "Alice"],
                    ["not a number", "Bob"],
                    ["40", "Carol"],
                ]);

            var report = await ValidateAsync(factory, path);

            Assert.Equal(3, report.RowCount);
            Assert.Equal(2, report.ColumnCount);
            Assert.Equal(1, report.InvalidCellCount);
            Assert.False(report.IsTruncated);

            var column = Assert.Single(report.Columns);

            Assert.Equal("Age", column.ColumnName);
            Assert.Equal(ColumnDataType.Integer, column.DataType);
            Assert.Equal(1, column.InvalidCount);

            var cell = Assert.Single(report.Cells);

            Assert.Equal(2, cell.RowPosition);
            Assert.Equal(1, cell.ColumnOrdinal);
            Assert.Equal("Age", cell.ColumnName);
            Assert.Equal("not a number", cell.Value);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Column_Of_Text_Takes_Anything_And_Is_Never_Reported()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            await WriteTypedThroughSinkAsync(
                factory,
                path,
                [new TableColumn("Name", ColumnDataType.Text)],
                [["123"], ["!!!"], ["anything at all"]]);

            var report = await ValidateAsync(factory, path);

            Assert.Equal(3, report.RowCount);
            Assert.Equal(0, report.InvalidCellCount);
            Assert.Empty(report.Columns);
            Assert.Empty(report.Cells);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task An_Empty_Value_Is_Not_A_Problem()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            // An empty cell, and a cell of nothing but space, both count as unset rather than as a value
            // that does not read as the column's type.
            await WriteTypedThroughSinkAsync(
                factory,
                path,
                [new TableColumn("Age", ColumnDataType.Integer)],
                [[""], ["   "], ["5"]]);

            var report = await ValidateAsync(factory, path);

            Assert.Equal(3, report.RowCount);
            Assert.Equal(0, report.InvalidCellCount);
            Assert.Empty(report.Columns);
            Assert.Empty(report.Cells);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task Only_The_Columns_That_Hold_Something_Faulty_Are_Listed()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            await WriteTypedThroughSinkAsync(
                factory,
                path,
                [new TableColumn("Age", ColumnDataType.Integer), new TableColumn("Score", ColumnDataType.Decimal)],
                [
                    ["30", "12.5"],
                    ["bad", "7"],
                    ["40", "not a number"],
                ]);

            var report = await ValidateAsync(factory, path);

            Assert.Equal(2, report.InvalidCellCount);
            Assert.Equal(2, report.Columns.Count);

            Assert.Equal("Age", report.Columns[0].ColumnName);
            Assert.Equal(1, report.Columns[0].InvalidCount);

            Assert.Equal("Score", report.Columns[1].ColumnName);
            Assert.Equal(1, report.Columns[1].InvalidCount);

            // The values are read a column at a time, so the fault under the first column is listed before
            // the one under the second.
            Assert.Equal(2, report.Cells.Count);
            Assert.Equal("Age", report.Cells[0].ColumnName);
            Assert.Equal(2, report.Cells[0].RowPosition);
            Assert.Equal("Score", report.Cells[1].ColumnName);
            Assert.Equal(3, report.Cells[1].RowPosition);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task More_Faults_Than_Asked_For_Are_Counted_But_Only_The_First_Are_Listed()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            await WriteTypedThroughSinkAsync(
                factory,
                path,
                [new TableColumn("Age", ColumnDataType.Integer)],
                [["a"], ["b"], ["c"], ["d"], ["e"]]);

            var report = await ValidateAsync(factory, path, maximumReportedCells: 2);

            // The counts always describe the whole table, while the list is only as long as was asked for.
            Assert.Equal(5, report.InvalidCellCount);
            Assert.Equal(5, report.Columns[0].InvalidCount);
            Assert.Equal(2, report.Cells.Count);
            Assert.True(report.IsTruncated);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Row_Is_Reported_By_Its_Place_In_The_Table_Not_By_Its_Id()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            await WriteTypedThroughSinkAsync(
                factory,
                path,
                [new TableColumn("Age", ColumnDataType.Integer)],
                [["10"], ["20"], ["bad"]]);

            // Deleting the first row leaves a gap in the ids that the rows after it do not move up to
            // fill, so the faulty row keeps its id while changing its place.
            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                var first = await dbContext.Rows.OrderBy(row => row.Id).FirstAsync();

                dbContext.Rows.Remove(first);

                await dbContext.SaveChangesAsync();
            }

            var report = await ValidateAsync(factory, path);

            var cell = Assert.Single(report.Cells);

            Assert.Equal("bad", cell.Value);
            Assert.Equal(2, cell.RowPosition);
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task An_Empty_Table_Has_Nothing_To_Report()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            await WriteTypedThroughSinkAsync(
                factory,
                path,
                [new TableColumn("Age", ColumnDataType.Integer)],
                []);

            var report = await ValidateAsync(factory, path);

            Assert.Equal(0, report.RowCount);
            Assert.Equal(1, report.ColumnCount);
            Assert.Equal(0, report.InvalidCellCount);
            Assert.Empty(report.Columns);
            Assert.Empty(report.Cells);
        }
        finally
        {
            DeleteStore(path);
        }
    }
}

