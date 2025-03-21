using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TableConverter.Converters;

public class DynamicNumericConverter : IValueConverter
{
    private readonly Type _TargetType;

    public DynamicNumericConverter(Type targetType)
    {
        if (!typeof(IConvertible).IsAssignableFrom(targetType))
            throw new ArgumentException("Target type must be a numeric type.", nameof(targetType));

        _TargetType = targetType;
    }

    // Convert from decimal to the target type
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return decimal.Zero; // Return default decimal value

        try
        {
            // Convert the value back to decimal
            var convertedValue = System.Convert.ChangeType(value, _TargetType, culture);
            return System.Convert.ToDecimal(convertedValue, culture);
        }
        catch (Exception)
        {
            return decimal.Zero; // Safe fallback if conversion fails
        }
    }

    // Convert back from target type to decimal
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return Activator.CreateInstance(_TargetType); // Return default value for null

        try
        {
            // Convert from decimal to the target numeric type
            var decimalValue = System.Convert.ToDecimal(value, culture);
            return System.Convert.ChangeType(decimalValue, _TargetType, culture);
        }
        catch (Exception)
        {
            return Activator.CreateInstance(_TargetType); // Safe fallback if conversion fails
        }
    }
}