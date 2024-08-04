using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace TableConverter.Converters
{
    public sealed class IsArrayEmptyConverter : IValueConverter
    {
        public static readonly IsArrayEmptyConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is IEnumerable enumerable)
            {
                return enumerable.Cast<object>().Any();
            }
            else
            {
                return new BindingNotification("Converter must be passed an enumerable collection.");
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
