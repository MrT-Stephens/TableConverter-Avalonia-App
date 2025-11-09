using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using TableConverter.ViewModels.Forms;

namespace TableConverter.Converters;

public class DockPositionToSizeConverter : IValueConverter
{
    public static readonly DockPositionToSizeConverter Instance = new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ToolsSettingsForm settingsForm)
            return new BindingNotification("Value is not of type ToolSettingsForm");
        
        if (parameter is not string type)
            return new BindingNotification("Parameter is not of type string");

        return settingsForm.Position switch
        {
            Dock.Bottom or Dock.Top => type == "Width" ? double.NaN : settingsForm.Size,
            Dock.Left or Dock.Right => type == "Width" ? settingsForm.Size : double.NaN,
            _ => settingsForm.Size
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}