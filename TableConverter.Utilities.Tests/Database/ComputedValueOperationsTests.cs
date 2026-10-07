using TableConverter.Utilities.Database;

namespace TableConverter.Utilities.Tests.Database;

/// <summary>
///     Covers the per-value arithmetic a derived column is built from. Each operation is a pure function of
///     the values handed to it, so what a derived column would hold is checked here without a store behind it.
/// </summary>
public class ComputedValueOperationsTests
{
    [Fact]
    public void Joining_Runs_The_Values_Together_With_The_Separator_Between_Them()
    {
        var result = ComputedValueOperations.Concatenate(["Ada", "Lovelace"], " ", skipBlank: false);

        Assert.Equal("Ada Lovelace", result);
    }

    [Fact]
    public void Joining_Can_Leave_Out_The_Values_That_Are_Unset()
    {
        var result = ComputedValueOperations.Concatenate(["Ada", null, "  ", "Lovelace"], ", ", skipBlank: true);

        Assert.Equal("Ada, Lovelace", result);
    }

    [Fact]
    public void Joining_Keeps_The_Place_Of_A_Value_That_Is_Unset_When_It_Is_Told_To()
    {
        var result = ComputedValueOperations.Concatenate(["Ada", null, "Lovelace"], "|", skipBlank: false);

        Assert.Equal("Ada||Lovelace", result);
    }

    [Fact]
    public void Joining_Runs_The_Values_Together_When_There_Is_No_Separator()
    {
        var result = ComputedValueOperations.Concatenate(["A", "B", "C"], null, skipBlank: false);

        Assert.Equal("ABC", result);
    }

    [Fact]
    public void Splitting_Takes_The_Part_At_The_Place_It_Is_Asked_For()
    {
        var result = ComputedValueOperations.SplitPart("a,b,c", ",", 1);

        Assert.Equal("b", result);
    }

    [Fact]
    public void Splitting_Reads_A_Place_The_Value_Has_Not_Got_As_Nothing()
    {
        var result = ComputedValueOperations.SplitPart("a,b", ",", 5);

        Assert.Null(result);
    }

    [Fact]
    public void Splitting_A_Value_That_Is_Unset_Reads_As_Nothing()
    {
        var result = ComputedValueOperations.SplitPart(null, ",", 0);

        Assert.Null(result);
    }

    [Fact]
    public void Taking_A_Run_Of_Characters_Gives_Back_Just_That_Run()
    {
        var result = ComputedValueOperations.ExtractSubstring("hello", 1, 3);

        Assert.Equal("ell", result);
    }

    [Fact]
    public void Taking_A_Run_With_No_Length_Runs_To_The_End_Of_The_Value()
    {
        var result = ComputedValueOperations.ExtractSubstring("hello", 1, -1);

        Assert.Equal("ello", result);
    }

    [Fact]
    public void Taking_A_Run_That_Reaches_Past_The_End_Stops_At_The_End()
    {
        var result = ComputedValueOperations.ExtractSubstring("hello", 2, 100);

        Assert.Equal("llo", result);
    }

    [Fact]
    public void Taking_A_Run_From_A_Value_That_Is_Unset_Reads_As_Nothing()
    {
        var result = ComputedValueOperations.ExtractSubstring(null, 0, 3);

        Assert.Null(result);
    }

    [Fact]
    public void Matching_Takes_The_Capture_It_Is_Asked_For()
    {
        var result = ComputedValueOperations.ExtractMatch("order 123 done", @"(\d+)", 1);

        Assert.Equal("123", result);
    }

    [Fact]
    public void Matching_A_Value_That_Does_Not_Match_Reads_As_Nothing()
    {
        var result = ComputedValueOperations.ExtractMatch("order done", @"(\d+)", 1);

        Assert.Null(result);
    }

    [Fact]
    public void Matching_With_A_Pattern_That_Cannot_Be_Read_Reads_As_Nothing()
    {
        var result = ComputedValueOperations.ExtractMatch("order 123", "([", 1);

        Assert.Null(result);
    }

    [Fact]
    public void Working_Out_Adds_The_Numbers_Of_A_Row()
    {
        var result = ComputedValueOperations.Calculate(["3", "4"], MathOperator.Add, 2);

        Assert.Equal("7", result);
    }

    [Fact]
    public void Working_Out_Takes_Each_Number_From_The_One_Before_It()
    {
        var result = ComputedValueOperations.Calculate(["10", "3", "1"], MathOperator.Subtract, 2);

        Assert.Equal("6", result);
    }

    [Fact]
    public void Working_Out_Rounds_The_Answer_To_The_Places_It_Is_Asked_For()
    {
        var result = ComputedValueOperations.Calculate(["10", "3"], MathOperator.Divide, 2);

        Assert.Equal("3.33", result);
    }

    [Fact]
    public void Working_Out_A_Division_By_Zero_Reads_As_Nothing()
    {
        var result = ComputedValueOperations.Calculate(["10", "0"], MathOperator.Divide, 2);

        Assert.Null(result);
    }

    [Fact]
    public void Working_Out_A_Value_That_Is_Not_A_Number_Reads_As_Nothing()
    {
        var result = ComputedValueOperations.Calculate(["10", "not a number"], MathOperator.Add, 2);

        Assert.Null(result);
    }

    [Fact]
    public void A_Date_Part_That_Is_A_Number_Comes_Back_As_That_Number()
    {
        var result = ComputedValueOperations.DatePart("2024-03-15", DatePartKind.Year);

        Assert.Equal("2024", result);
    }

    [Fact]
    public void The_Quarter_Of_A_Moment_Is_Counted_From_The_Start_Of_The_Year()
    {
        var result = ComputedValueOperations.DatePart("2024-05-15", DatePartKind.Quarter);

        Assert.Equal("2", result);
    }

    [Fact]
    public void A_Date_Part_That_Is_A_Name_Comes_Back_As_That_Name()
    {
        var result = ComputedValueOperations.DatePart("2024-03-15", DatePartKind.MonthName);

        Assert.Equal("March", result);
    }

    [Fact]
    public void A_Value_That_Does_Not_Read_As_A_Moment_Reads_As_Nothing()
    {
        var result = ComputedValueOperations.DatePart("not a date", DatePartKind.Year);

        Assert.Null(result);
    }

    [Fact]
    public void A_Part_That_Is_A_Name_Is_Not_Read_As_A_Number()
    {
        Assert.False(ComputedValueOperations.IsNumericPart(DatePartKind.MonthName));
        Assert.False(ComputedValueOperations.IsNumericPart(DatePartKind.WeekdayName));
    }

    [Fact]
    public void A_Part_That_Is_A_Number_Is_Read_As_A_Number()
    {
        Assert.True(ComputedValueOperations.IsNumericPart(DatePartKind.Year));
        Assert.True(ComputedValueOperations.IsNumericPart(DatePartKind.DayOfYear));
    }
}

