using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace TableConverter.Converters;

public class DateOnlyToDateTimeOffsetConverter : IValueConverter
{
    public static readonly DateOnlyToDateTimeOffsetConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateOnly dateOnly) return new DateTimeOffset(dateOnly.ToDateTime(TimeOnly.MinValue));

        return new BindingNotification("Value is not a DateOnly.");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTimeOffset dateTimeOffset) return DateOnly.FromDateTime(dateTimeOffset.DateTime);

        return new BindingNotification("Value is not a DateTimeOffset.");
    }
}