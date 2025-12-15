using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace TableConverter.Extensions;

public class IconFactoryExtension : MarkupExtension
{
    public string Path { get; set; } = string.Empty;
    public double Size { get; set; } = 16;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var geometry = Application.Current?.Resources[Path] as StreamGeometry
            ?? throw new InvalidOperationException("Icon not found");
        
        return new PathIcon
        {
            Data = geometry,
            Width = Size,
            Height = Size
        };
    }
}