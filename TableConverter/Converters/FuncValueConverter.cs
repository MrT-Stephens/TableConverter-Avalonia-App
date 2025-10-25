using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Utilities;

namespace TableConverter.Converters;

public class FuncValueConverter<TOut> : IValueConverter
{
    private readonly Func<object?, TOut> _convert;
    
    public FuncValueConverter(Func<object?, TOut> convert)
    {
        _convert = convert;
    }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (TypeUtilities.CanCast<object>(value))
        {
            return _convert(value);
        }

        return AvaloniaProperty.UnsetValue;
    }
    
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (TypeUtilities.CanCast<TOut>(value))
        {
            return _convert(value);
        }

        return AvaloniaProperty.UnsetValue;
    }
}