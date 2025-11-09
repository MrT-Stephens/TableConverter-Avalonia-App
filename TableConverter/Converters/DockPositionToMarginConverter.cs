using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace TableConverter.Converters;

public class DockPositionToMarginConverter : IValueConverter
{
    public static readonly DockPositionToMarginConverter Instance = new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Dock position)
            return new BindingNotification("Value is not the type of Dock");

        if (!double.TryParse(parameter?.ToString(), out var spacing))
            spacing = 100;

        return position switch
        {
            Dock.Top => new Thickness(0, 0, 0, spacing),
            Dock.Bottom => new Thickness(0, spacing, 0, 0),
            Dock.Left => new Thickness(0, 0, spacing, 0),
            Dock.Right => new Thickness(spacing, 0, 0, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(position), position, null)
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}