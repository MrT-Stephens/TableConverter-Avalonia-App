using Avalonia.Controls;
using TableConverter.Components.Extensions.Interfaces;

namespace TableConverter.Components.Extensions.Definitions;

public class SpacedRowDefinition : RowDefinition, ISpacing
{
    public SpacedRowDefinition(double height) : base(height, GridUnitType.Pixel)
    {
    }

    public double Spacing
    {
        get => Height.Value;
        set => Height = new GridLength(value, GridUnitType.Pixel);
    }
}