using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TableConverter.Converters
{
    public sealed class ArrayToStringConverter : IValueConverter
    {
        public static readonly ArrayToStringConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is IEnumerable<object?> enumerable) 
            { 
                var count = enumerable.Count();

                if (!enumerable.Any())
                {
                    return new BindingNotification("Array to string converter must have elements");
                }

                if (count == 1)
                {
                    return enumerable.First();
                }

                if (count == 2)
                {
                    return string.Join(" or ", enumerable);
                }

                return $"{string.Join(", ", enumerable.Take(count - 1))} or {enumerable.Last()}";
            }

            return new BindingNotification("Array to string converter must be passed an array");
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
