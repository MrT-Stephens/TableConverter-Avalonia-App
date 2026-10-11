using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.Extensions;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Utilities.Interfaces;
using TableConverter.Utilities.Models;

namespace TableConverter.Utilities.Tests.Database;

/// <summary>
/// Covers column types: how a value is tested against the type its column was given, how a column that
/// named no type has one read off its values, and how the type itself is kept in the store.
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
    [InlineData(ColumnDataType.Date, "2026-09-26T00:00:00")]
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
    [InlineData(ColumnDataType.Date, "2026-09-26T14:30:00")]
    [InlineData(ColumnDataType.Date, "2026-09-26 14:30:00")]
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
    public async Task A_Column_That_Reads_As_Nothing_Stays_Text()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            // A column whose values do not read as any kind of value is left as text, so an imported
            // table never ends up with a type that reading the values could not justify.
            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                await using var sink = TableStoreRowSink.Create(dbContext);

                await sink.BeginAsync(["A", "B"]);
                await sink.WriteRowAsync(["a1", "b1"]);
                await sink.CompleteAsync();
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

    [Fact]
    public async Task A_Column_That_Named_No_Type_Is_Typed_From_The_Values_Written_To_It()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            // This is how an imported or generated table stops being made of nothing but text: the values
            // are read to find out what each column holds.
            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                await using var sink = TableStoreRowSink.Create(dbContext);

                await sink.BeginAsync(["Whole", "Fraction", "Flag", "Day", "Moment"]);

                await sink.WriteRowAsync(["42", "1234.56", "true", "2026-09-26", "2026-09-26T14:30:00"]);
                await sink.WriteRowAsync(["7", "-0.5", "False", "2025-01-02", "2025-01-02T09:00:00"]);

                await sink.CompleteAsync();
            }

            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                var types = await dbContext.Columns
                    .AsNoTracking()
                    .OrderBy(column => column.OrdinalPosition)
                    .Select(column => column.DataType)
                    .ToListAsync();

                Assert.Equal(
                [
                    ColumnDataType.Integer,
                    ColumnDataType.Decimal,
                    ColumnDataType.Boolean,
                    ColumnDataType.Date,
                    ColumnDataType.DateTime
                ], types);
            }
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Column_Whose_Values_Do_Not_Agree_Stays_Text()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            // One value that does not fit is enough to leave the column as text, so a column is never
            // mistyped on the strength of the values that happen to read.
            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                await using var sink = TableStoreRowSink.Create(dbContext);

                await sink.BeginAsync(["Amount"]);

                await sink.WriteRowAsync(["42"]);
                await sink.WriteRowAsync(["not a number"]);

                await sink.CompleteAsync();
            }

            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                var type = await dbContext.Columns.AsNoTracking().Select(column => column.DataType).SingleAsync();

                Assert.Equal(ColumnDataType.Text, type);
            }
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Column_Whose_Type_Was_Chosen_Keeps_It()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            // A column that named its type is taken at its word, so a text column of numbers stays a text
            // column rather than being read back as something the person who set it did not ask for.
            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                await using var sink = TableStoreRowSink.Create(dbContext);

                await sink.BeginAsync([new TableColumn("Code", ColumnDataType.Text)]);

                await sink.WriteRowAsync(["0042"]);

                await sink.CompleteAsync();
            }

            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                var type = await dbContext.Columns.AsNoTracking().Select(column => column.DataType).SingleAsync();

                Assert.Equal(ColumnDataType.Text, type);
            }
        }
        finally
        {
            DeleteStore(path);
        }
    }

    [Fact]
    public async Task A_Column_Of_Timestamps_Is_A_Date_And_Time_Column()
    {
        using var provider = BuildProvider();
        var factory = provider.GetRequiredService<ITableStoreDbContextFactory>();

        var path = NewStorePath();

        try
        {
            // A timestamp is a date and a time, so reading it as a date would quietly drop the time the
            // moment it was shown or exported.
            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                await using var sink = TableStoreRowSink.Create(dbContext);

                await sink.BeginAsync(["Taken"]);

                await sink.WriteRowAsync(["2026-09-26T14:30:00"]);

                await sink.CompleteAsync();
            }

            await using (var dbContext = await factory.CreateDbContextAsync(path))
            {
                var type = await dbContext.Columns.AsNoTracking().Select(column => column.DataType).SingleAsync();

                Assert.Equal(ColumnDataType.DateTime, type);
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

