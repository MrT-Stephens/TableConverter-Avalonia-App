using System.Globalization;
using Avalonia.Controls;
using Avalonia.Media;
using TableConverter.Converters;
using TableConverter.Services.DataSources.Base;
using TableConverter.Utilities.Models;
using TableConverter.Views.Controls.CellEditors;

namespace TableConverter.Tests.Converters;

/// <summary>
/// Covers how a cell is shown once its column has been given a type: the value laid out the way the type
/// reads, a column of numbers lined up on the right, and a value that does not read as the type marked
/// rather than rejected.
/// </summary>
public class ColumnValueConvertersTests
{
    private static object? Format(ColumnDataType dataType, string? value)
    {
        return new ColumnValueFormatConverter(dataType)
            .Convert(value, typeof(string), null, CultureInfo.InvariantCulture);
    }

    private static object? IsMismatched(ColumnDataType dataType, string? value)
    {
        return new ColumnValueMismatchConverter(dataType)
            .Convert(value, typeof(bool), null, CultureInfo.InvariantCulture);
    }

    [Theory]
    [InlineData(ColumnDataType.Integer, "1234", "1,234")]
    [InlineData(ColumnDataType.Integer, "-7", "-7")]
    [InlineData(ColumnDataType.Decimal, "1234.5", "1,234.5")]
    [InlineData(ColumnDataType.Decimal, "-0.5", "-0.5")]
    [InlineData(ColumnDataType.Boolean, "true", "True")]
    [InlineData(ColumnDataType.Date, "2026-09-26", "2026-09-26")]
    [InlineData(ColumnDataType.DateTime, "2026-09-26T14:30:00", "2026-09-26 14:30:00")]
    [InlineData(ColumnDataType.Text, "anything at all", "anything at all")]
    public void A_Value_Is_Shown_The_Way_Its_Column_Type_Reads(ColumnDataType dataType, string value, string expected)
    {
        Assert.Equal(expected, Format(dataType, value));
    }

    [Theory]
    [InlineData(ColumnDataType.Integer, "12.5")]
    [InlineData(ColumnDataType.Decimal, "not a number")]
    [InlineData(ColumnDataType.Date, "not a date")]
    public void A_Value_That_Does_Not_Read_As_Its_Type_Is_Shown_Exactly_As_It_Was_Typed(
        ColumnDataType dataType,
        string value)
    {
        // What is wrong with the value can only be seen if the value is shown as it stands, so nothing is
        // tidied up for display.
        Assert.Equal(value, Format(dataType, value));
    }

    [Fact]
    public void Showing_A_Value_Does_Not_Change_What_Is_Stored()
    {
        // The formatting is display only: converting back hands over the text as it was, so a value is
        // never quietly rewritten by having been looked at.
        var converter = new ColumnValueFormatConverter(ColumnDataType.Integer);

        // A value written with its thousands grouped is not stripped back to digits by having been shown,
        // so what is stored stays exactly what was typed.
        Assert.Equal("1,234", converter.ConvertBack("1,234", typeof(string), null, CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData(ColumnDataType.Integer, "12", false)]
    [InlineData(ColumnDataType.Integer, "12.5", true)]
    [InlineData(ColumnDataType.Integer, "twelve", true)]
    [InlineData(ColumnDataType.Decimal, "12.5", false)]
    [InlineData(ColumnDataType.Boolean, "yes", true)]
    [InlineData(ColumnDataType.Date, "2026-09-26", false)]
    [InlineData(ColumnDataType.Date, "2026-09-26T14:30:00", true)]
    [InlineData(ColumnDataType.Text, "anything at all", false)]
    public void A_Value_Is_Marked_When_It_Does_Not_Read_As_Its_Column_Type(
        ColumnDataType dataType,
        string value,
        bool expected)
    {
        Assert.Equal(expected, IsMismatched(dataType, value));
    }

    [Fact]
    public void A_Missing_Value_Is_Never_Marked()
    {
        // A cell is allowed to hold nothing whatever its column's type is, so an empty cell is not a
        // value of the wrong type.
        Assert.Equal(false, IsMismatched(ColumnDataType.Integer, null));
        Assert.Equal(false, IsMismatched(ColumnDataType.Integer, string.Empty));
    }

    [Fact]
    public void A_Placeholder_Is_Never_Marked()
    {
        // A placeholder stands in for a value that has not been read yet, so it says nothing about whether
        // the column's type fits its data.
        Assert.Equal(false, IsMismatched(ColumnDataType.Integer, DataSourcePlaceholder.Text));
    }

    [Fact]
    public void A_Column_Of_Numbers_Is_Lined_Up_On_The_Right()
    {
        var display = Assert.IsType<TextBlock>(
            CellEditorFactory.CreateDisplay(ColumnDataType.Decimal, "Item.Cells[0].Value"));

        Assert.Equal(TextAlignment.Right, display.TextAlignment);
    }

    [Theory]
    [InlineData(ColumnDataType.Text)]
    [InlineData(ColumnDataType.Boolean)]
    [InlineData(ColumnDataType.Date)]
    [InlineData(ColumnDataType.DateTime)]
    public void A_Column_That_Is_Not_A_Column_Of_Numbers_Is_Lined_Up_On_The_Left(ColumnDataType dataType)
    {
        var display = Assert.IsType<TextBlock>(
            CellEditorFactory.CreateDisplay(dataType, "Item.Cells[0].Value"));

        Assert.Equal(TextAlignment.Left, display.TextAlignment);
    }
}
