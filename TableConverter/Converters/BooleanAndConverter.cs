using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace TableConverter.Converters
{
    public class BooleanAndConverter : IMultiValueConverter
    {
        public static readonly BooleanAndConverter Instance = new();

        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values is null || values.Count == 0)
            {
                return new BindingNotification("List of values is empty or null");
            }

            foreach (var value in values)
            {
                if (bool.TryParse(value?.ToString(), out bool result) && !result)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
