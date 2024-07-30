using Avalonia.Controls;
using TableConverter.Components.Interfaces;

namespace TableConverter.Components.Definitions
{
    public class SpacedColumnDefinition : ColumnDefinition, ISpacing
    {
        public double Spacing
        {
            get => Width.Value;
            set => Width = new GridLength(value, GridUnitType.Pixel);
        }

        public SpacedColumnDefinition(double width) : base(width, GridUnitType.Pixel)
        {
        }
    }
}
