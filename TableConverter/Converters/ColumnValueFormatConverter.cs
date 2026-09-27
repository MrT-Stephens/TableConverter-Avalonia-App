using System;
using System.Globalization;
using Avalonia.Data.Converters;
using TableConverter.Utilities.Models;

namespace TableConverter.Converters;

/// <summary>
/// Shows a cell's text the way its column's type reads.
/// </summary>
/// <remarks>
/// <para>
/// A column of numbers reads as a column of numbers: the thousands are grouped, so the size of a value
/// can be taken in at a glance, and dates are shown in the one form that cannot be misread.
/// </para>
/// <para>
/// This is display only. Converting back gives the text back exactly as it was, so what is stored is
/// always what the user typed and a value is never quietly rewritten by being looked at.
/// </para>
/// </remarks>
public class ColumnValueFormatConverter(ColumnDataType dataType) : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return dataType.FormatValue(value as string);
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // The stored value is what was typed, never its formatting, so the text is handed back as it is.
        return value;
    }
}

