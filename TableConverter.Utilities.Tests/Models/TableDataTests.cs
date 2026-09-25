using TableConverter.Utilities;

namespace TableConverter.Utilities.Tests.Models;

public class TableDataTests
{
    private static TableData CreateSample()
    {
        return new TableData(["Name", "Age"], [["Alice", "30"], ["Bob", "25"]]);
    }

    [Fact]
    public void Equals_Returns_True_For_Identical_Tables()
    {
        var first = CreateSample();
        var second = CreateSample();

        Assert.True(first.Equals(second));
        Assert.True(second.Equals(first));
        Assert.Equal(first, second);
    }

    [Fact]
    public void Equals_Returns_False_When_Other_Has_Fewer_Rows()
    {
        // Regression: this used to throw IndexOutOfRangeException instead of returning false.
        var first = CreateSample();
        var second = new TableData(["Name", "Age"], [["Alice", "30"]]);

        Assert.False(first.Equals(second));
        Assert.False(second.Equals(first));
    }

    [Fact]
    public void Equals_Returns_False_When_Other_Has_More_Rows()
    {
        var first = new TableData(["Name", "Age"], [["Alice", "30"]]);
        var second = CreateSample();

        Assert.False(first.Equals(second));
        Assert.False(second.Equals(first));
    }

    [Fact]
    public void Equals_Returns_False_When_Other_Has_Fewer_Headers()
    {
        var first = CreateSample();
        var second = new TableData(["Name"], [["Alice", "30"], ["Bob", "25"]]);

        Assert.False(first.Equals(second));
        Assert.False(second.Equals(first));
    }

    [Fact]
    public void Equals_Returns_False_When_Cell_Values_Differ()
    {
        var first = CreateSample();
        var second = new TableData(["Name", "Age"], [["Alice", "30"], ["Bob", "26"]]);

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Equals_Returns_False_When_Row_Lengths_Differ()
    {
        var first = new TableData(["Name", "Age"], [["Alice", "30"]]);
        var second = new TableData(["Name", "Age"], [["Alice"]]);

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Equals_Returns_False_For_Null_Or_Other_Types()
    {
        var table = CreateSample();

        Assert.False(table.Equals(null));
        Assert.False(table.Equals("not a table"));
    }

    [Fact]
    public void Equal_Tables_Produce_Equal_Hash_Codes()
    {
        // Regression: GetHashCode used to be reference based, breaking the Equals/GetHashCode contract.
        var first = CreateSample();
        var second = CreateSample();

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Different_Content_Usually_Produces_Different_Hash_Codes()
    {
        var first = CreateSample();
        var second = new TableData(["Name", "Age"], [["Alice", "30"], ["Carol", "25"]]);

        Assert.NotEqual(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Hash_Codes_Are_Stable_Across_Calls()
    {
        var table = CreateSample();

        Assert.Equal(table.GetHashCode(), table.GetHashCode());
    }

    [Fact]
    public void Tables_Can_Be_Used_As_Dictionary_Keys()
    {
        var dictionary = new Dictionary<TableData, string>
        {
            [CreateSample()] = "value"
        };

        Assert.True(dictionary.TryGetValue(CreateSample(), out var value));
        Assert.Equal("value", value);
    }

    [Fact]
    public void Copy_Constructor_Deep_Copies_Rows()
    {
        // Regression: mutating a copied table used to mutate the original (shared row arrays).
        var original = CreateSample();
        var copy = new TableData(original);

        copy.Rows[0][0] = "changed";
        copy.Headers[0] = "changed";

        Assert.Equal("Alice", original.Rows[0][0]);
        Assert.Equal("Name", original.Headers[0]);
    }

    [Fact]
    public void Ragged_Tables_Can_Be_Compared_Without_Throwing()
    {
        var first = new TableData(["A", "B", "C"], [["1"]]);
        var second = new TableData(["A", "B", "C"], [["1", "2", "3"]]);

        Assert.False(first.Equals(second));
        Assert.False(second.Equals(first));

        // Hash codes must not throw for ragged rows either.
        _ = first.GetHashCode();
        _ = second.GetHashCode();
    }

    [Fact]
    public void Empty_Tables_Are_Equal()
    {
        var first = new TableData([], []);
        var second = new TableData([], []);

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }
}

