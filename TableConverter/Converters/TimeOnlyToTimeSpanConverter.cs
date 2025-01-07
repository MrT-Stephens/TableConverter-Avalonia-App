using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace TableConverter.Converters;

public class TimeOnlyToTimeConverter : IValueConverter
{
    public static readonly TimeOnlyToTimeConverter Instance = new();
    
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeOnly timeOnly)
        {
            return timeOnly.ToTimeSpan(); // Convert TimeOnly to TimeSpan
        }
        
        return new BindingNotification("Value is not a TimeOnly.");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeSpan timeSpan)
        {
            return TimeOnly.FromTimeSpan(timeSpan); // Convert TimeSpan back to TimeOnly
        }
        
        return new BindingNotification("Value is not a TimeSpan.");
    }
}