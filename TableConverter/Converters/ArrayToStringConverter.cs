using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace TableConverter.Converters;

public sealed class ArrayToStringConverter : IValueConverter
{
    public static readonly ArrayToStringConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IEnumerable<object?> enumerable)
        {
            var objects = enumerable as object[] ?? enumerable.ToArray();
            
            var count = objects.Length;

            if (!objects.Any()) return new BindingNotification("Array to string converter must have elements");

            if (count == 1) return objects.First();

            if (count == 2) return string.Join(" or ", objects);

            return $"{string.Join(", ", objects.Take(count - 1))} or {objects.Last()}";
        }

        return new BindingNotification("Array to string converter must be passed an array");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}