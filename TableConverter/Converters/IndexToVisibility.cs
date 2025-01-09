using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TableConverter.Converters;

public class IndexToVisibility : IValueConverter
{
    public static readonly IndexToVisibility Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int currentIndex && parameter is string stepParam && int.TryParse(stepParam, out var stepIndex))
            return currentIndex == stepIndex;

        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}