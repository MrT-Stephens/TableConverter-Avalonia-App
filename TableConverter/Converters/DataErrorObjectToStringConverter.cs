using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TableConverter.Converters;

public class DataErrorObjectToStringConverter : IValueConverter
{
    public static readonly DataErrorObjectToStringConverter Instance = new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Exception ex)
        {
            return ex.Message;
        }

        return value?.ToString() ?? string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}