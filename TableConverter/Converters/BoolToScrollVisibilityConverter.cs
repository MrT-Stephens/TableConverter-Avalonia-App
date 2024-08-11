using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace TableConverter.Converters
{
    public sealed class BoolToScrollVisibilityConverter : IValueConverter
    {
        public static readonly BoolToScrollVisibilityConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolean)
            {
                return boolean ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
            }
            else
            {
                return new BindingNotification("Bool to scroll visibility converter must be passed an boolean");
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
