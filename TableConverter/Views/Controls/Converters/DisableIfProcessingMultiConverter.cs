using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TableConverter.Views.Controls.Converters;

public class DisableIfProcessingMultiConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count is not 2)
        {
            return true;
        }

        var disableAll = values[0] as bool? ?? false;
        var anyProcessing = values[1] as bool? ?? false;

        return !(disableAll && anyProcessing);
    }
}