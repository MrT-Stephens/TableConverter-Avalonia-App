using System;
using System.Globalization;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;

namespace TableConverter.Converters;

public class BoolToScrollBarVisibilityConverter : IValueConverter
{
    public static readonly BoolToScrollBarVisibilityConverter Instance = new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
        }

        return ScrollBarVisibility.Disabled;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ScrollBarVisibility visibility)
        {
            return visibility == ScrollBarVisibility.Auto;
        }

        return false;
    }
}