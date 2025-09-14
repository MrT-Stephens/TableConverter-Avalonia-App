using Avalonia.Controls;
using TableConverter.Views.Controls.Panels.Interfaces;

namespace TableConverter.Views.Controls.Panels.Definitions;

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