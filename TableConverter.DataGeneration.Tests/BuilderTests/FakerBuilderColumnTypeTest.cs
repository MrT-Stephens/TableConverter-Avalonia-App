using TableConverter.DataGeneration.Interfaces;
using TableConverter.Utilities.Models;

namespace TableConverter.DataGeneration.Tests.BuilderTests;

/// <summary>
///     Covers the kind of value the builder declares each column of a generated table with.
/// </summary>
/// <remarks>
///     A generated table is the one table whose types are known rather than read off its values: the
///     builder knows what its generators produce, so what it declares is what the rest of the
///     application works from.
/// </remarks>
public class FakerBuilderColumnTypeTest
{
    [Fact]
    public async Task Build_Declares_A_Column_Added_Without_A_Type_As_Text()
    {
        var snapshot = await Build(builder => builder.Add("City", faker => faker.Location.City()));

        Assert.Equal(ColumnDataType.Text, Assert.Single(snapshot.Columns).DataType);
    }

    [Fact]
    public async Task Build_Declares_A_Column_With_The_Type_Its_Generator_Names()
    {
        var snapshot = await Build(builder =>
            builder.Add("Price", ColumnDataType.Decimal, faker => faker.Commerce.Price()));

        Assert.Equal(ColumnDataType.Decimal, Assert.Single(snapshot.Columns).DataType);
    }

    [Fact]
    public async Task Build_Declares_A_Column_And_Its_Own_Type_For_Every_Generator_Of_A_Name()
    {
        var snapshot = await Build(builder => builder
            .Add("Amount", ColumnDataType.Integer, faker => faker.Number.Integer(1, 1000))
            .Add("Amount", ColumnDataType.Decimal, faker => faker.Number.Decimal(0, 1000)));

        Assert.Equal(2, snapshot.Columns.Count);
        Assert.Equal(new TableColumn("Amount", ColumnDataType.Integer), snapshot.Columns[0]);
        Assert.Equal(new TableColumn("Amount", ColumnDataType.Decimal), snapshot.Columns[1]);
    }

    [Fact]
    public async Task Build_Declares_A_Type_Even_When_Every_Value_Of_The_Column_Is_Blank()
    {
        var snapshot = await Build(builder =>
            builder.Add("Amount", ColumnDataType.Integer, faker => faker.Number.Integer(), blanksPercentage: 100));

        Assert.Equal(ColumnDataType.Integer, Assert.Single(snapshot.Columns).DataType);
        Assert.Equal(string.Empty, Assert.Single(Assert.Single(snapshot.Rows)));
    }

    [Fact]
    public async Task Build_Declares_A_Conditional_Column_As_Text_By_Default()
    {
        var snapshot = await Build(builder => builder
            .AddConditional("Note", faker => faker.Randomizer.Bool(), faker => faker.Lorem.Word()));

        Assert.Equal(ColumnDataType.Text, Assert.Single(snapshot.Columns).DataType);
    }

    [Fact]
    public async Task Build_Declares_A_Conditional_Column_With_The_Type_It_Names()
    {
        var snapshot = await Build(builder => builder
            .AddConditional("Amount", faker => faker.Randomizer.Bool(), faker => faker.Number.Integer(),
                dataType: ColumnDataType.Integer));

        Assert.Equal(ColumnDataType.Integer, Assert.Single(snapshot.Columns).DataType);
    }

    [Fact]
    public async Task Build_Writes_A_Cell_For_Every_Column_It_Declares()
    {
        var snapshot = await Build(
            builder => builder
                .Add("First Name", faker => faker.Person.FirstName())
                .Add("Amount", ColumnDataType.Integer, faker => faker.Number.Integer(1, 99)),
            rowCount: 3);

        Assert.True(snapshot.IsCompleted);
        Assert.Equal(2, snapshot.Columns.Count);
        Assert.Equal(3, snapshot.Rows.Count);
        Assert.All(snapshot.Rows, row => Assert.Equal(snapshot.Columns.Count, row.Length));
    }

    [Fact]
    public async Task Build_Declared_Numeric_Values_Read_Back_As_Numbers()
    {
        var snapshot = await Build(builder => builder
            .Add("Amount", ColumnDataType.Integer, faker => faker.Number.Integer(1, 1000))
            .Add("Amount", ColumnDataType.Decimal, faker => faker.Number.Decimal(0, 1000)));

        Assert.True(snapshot.Columns[0].TryReadValue(snapshot.Rows[0][0], out var amount));
        Assert.IsType<long>(amount);

        Assert.True(snapshot.Columns[1].TryReadValue(snapshot.Rows[0][1], out var rate));
        Assert.IsType<decimal>(rate);
    }

    /// <summary>
    ///     Builds a table through <paramref name="configure" /> and hands back what the builder declared
    ///     and wrote.
    /// </summary>
    /// <param name="configure">Sets out the columns of the table.</param>
    /// <param name="rowCount">The number of rows to generate. Defaults to one.</param>
    private static async Task<TableSnapshot> Build(
        Func<IFakerBuilder<Faker>, IFakerBuilder<Faker>> configure, int rowCount = 1)
    {
        var faker = new Faker();
        faker.Seed(20260927);

        var snapshot = new TableSnapshot();

        await configure(Faker.Create(faker)).WithRowCount(rowCount).BuildAsync(snapshot);

        return snapshot;
    }
}
