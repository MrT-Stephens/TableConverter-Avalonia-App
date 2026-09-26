using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.Extensions;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities.Tests.Database;

/// <summary>
/// Covers column types: how a value is tested against the type its column was given, and how the type
/// itself is kept in the store.
/// </summary>
public class ColumnDataTypeTests
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

    [Theory]
    [InlineData(ColumnDataType.Text, "anything at all")]
    [InlineData(ColumnDataType.Integer, "42")]
    [InlineData(ColumnDataType.Integer, "-7")]
    [InlineData(ColumnDataType.Integer, "1,234")]
    [InlineData(ColumnDataType.Decimal, "1234.56")]
    [InlineData(ColumnDataType.Decimal, "1,234.56")]
    [InlineData(ColumnDataType.Decimal, "-0.5")]
    [InlineData(ColumnDataType.Boolean, "true")]
    [InlineData(ColumnDataType.Boolean, "False")]
    [InlineData(ColumnDataType.Date, "2026-09-26")]
    [InlineData(ColumnDataType.DateTime, "2026-09-26T14:30:00")]
    public void IsValidValue_Accepts_Values_That_Read_As_The_Type(ColumnDataType dataType, string value)
    {
        Assert.True(dataType.IsValidValue(value));
    }

    [Theory]
    [InlineData(ColumnDataType.Integer, "12.5")]
    [InlineData(ColumnDataType.Integer, "twelve")]
    [InlineData(ColumnDataType.Decimal, "twelve")]
    [InlineData(ColumnDataType.Boolean, "yes")]
    [InlineData(ColumnDataType.Boolean, "1")]
    [InlineData(ColumnDataType.Date, "not a date")]
    [InlineData(ColumnDataType.DateTime, "not a date")]
    public void IsValidValue_Rejects_Values_That_Do_Not_Read_As_The_Type(ColumnDataType dataType, string value)
    {
        Assert.False(dataType.IsValidValue(value));
    }

    [Theory]
    [InlineData(ColumnDataType.Text)]
    [InlineData(ColumnDataType.Integer)]
    [InlineData(ColumnDataType.Decimal)]
    [InlineData(ColumnDataType.Boolean)]
    [InlineData(ColumnDataType.Date)]
    [InlineData(ColumnDataType.DateTime)]
    public void IsValidValue_Accepts_A_Missing_Value_Whatever_The_Type(ColumnDataType dataType)
    {
        // A cell is allowed to hold nothing whatever its column's type is, so an empty cell is never
        // flagged as the wrong type.
        Assert.True(dataType.IsValidValue(null));
        Assert.True(dataType.IsValidValue(string.Empty));
    }

    [Theory]
    [InlineData(ColumnDataType.Integer, true)]
    [InlineData(ColumnDataType.Decimal, true)]
    [InlineData(ColumnDataType.Text, false)]
    [InlineData(ColumnDataType.Boolean, false)]
    [InlineData(ColumnDataType.Date, false)]
    [InlineData(ColumnDataType.DateTime, false)]
    public void IsNumeric_Is_True_Only_For_Numeric_Types(ColumnDataType dataType, bool expected)
    {
        Assert.Equal(expected, dataType.IsNumeric());
    }

    [Fact]
    public async Task Column_Type_Round_Trips_Through_The_Store()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                dbContext.Columns.Add(new ColumnEntity
                {
                    Name = "Amount",
                    OrdinalPosition = 1,
                    DataType = ColumnDataType.Decimal
                });

                await dbContext.SaveChangesAsync();
            }

            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                var column = await dbContext.Columns.AsNoTracking().SingleAsync();

                Assert.Equal(ColumnDataType.Decimal, column.DataType);

                // The type travels as its numeric value, so the on disk format cannot be broken by
                // renaming an enum member.
                var stored = await dbContext.Database
                    .SqlQueryRaw<int>("SELECT DATA_TYPE AS Value FROM COLUMNS")
                    .SingleAsync();

                Assert.Equal((int)ColumnDataType.Decimal, stored);
            }
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Store_Written_From_A_Table_Is_Typed_As_Text()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            // An imported table carries no type information, so it must not leave columns with a type
            // that was never chosen.
            var tableData = new TableData(["A", "B"], [["a1", "b1"]]);

            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                await TableStoreDataWriter.WriteAsync(dbContext, tableData);
            }

            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                var types = await dbContext.Columns
                    .AsNoTracking()
                    .OrderBy(column => column.OrdinalPosition)
                    .Select(column => column.DataType)
                    .ToListAsync();

                Assert.Equal([ColumnDataType.Text, ColumnDataType.Text], types);
            }
        }
        finally
        {
            DeleteStore(path);
        }
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
}

