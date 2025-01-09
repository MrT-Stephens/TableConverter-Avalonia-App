using Avalonia.Controls;
using TableConverter.Components.Extensions.Interfaces;

namespace TableConverter.Components.Extensions.Definitions;

public class SpacedColumnDefinition : ColumnDefinition, ISpacing
{
    public SpacedColumnDefinition(double width) : base(width, GridUnitType.Pixel)
    {
    }

    public double Spacing
    {
        get => Width.Value;
        set => Width = new GridLength(value, GridUnitType.Pixel);
    }
}