using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace TableConverter.Converters;

public sealed class MergeValuesWithSeparatorConverter : IMultiValueConverter
{
    public static readonly MergeValuesWithSeparatorConverter Instance = new();
    
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        // Use the provided separator or default to ", "
        var separator = parameter as string ?? ", ";
        return string.Join(separator, values);
    }

    public object ConvertBack(IList<object> values, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}