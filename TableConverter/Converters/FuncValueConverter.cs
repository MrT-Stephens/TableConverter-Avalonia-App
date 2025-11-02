using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Utilities;

namespace TableConverter.Converters;

public class FuncValueConverter<TOut>(Func<object?, TOut> convert) : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return TypeUtilities.CanCast<object>(value) 
            ? convert(value) 
            : AvaloniaProperty.UnsetValue;
    }
    
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return TypeUtilities.CanCast<TOut>(value) 
            ? convert(value) 
            : AvaloniaProperty.UnsetValue;
    }
}