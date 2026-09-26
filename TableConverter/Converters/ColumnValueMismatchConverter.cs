using System;
using System.Globalization;
using Avalonia.Data.Converters;
using TableConverter.Services.DataSources.Base;
using TableConverter.Utilities.Database.Models.TableStore;

namespace TableConverter.Converters;

/// <summary>
/// Tells whether a cell's text reads as its column's type.
/// </summary>
/// <remarks>
/// The answer is a state rather than a brush, so that the colour standing for it is chosen by the
/// styles and can therefore be a theme colour. Nothing is rejected on the strength of it: the store
/// keeps whatever the user typed, so a half finished entry is not thrown away while it is being made.
/// </remarks>
public class ColumnValueMismatchConverter(ColumnDataType dataType) : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var text = value as string;

        // A placeholder stands in for a value that has not been read yet, so it says nothing about
        // whether the column's type fits its data.
        return text != DataSourcePlaceholder.Text && !dataType.IsValidValue(text);
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

