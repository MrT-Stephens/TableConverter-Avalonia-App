using Avalonia.Controls;
using TableConverter.Views.Controls.Panels.Interfaces;

namespace TableConverter.Views.Controls.Panels.Definitions;

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