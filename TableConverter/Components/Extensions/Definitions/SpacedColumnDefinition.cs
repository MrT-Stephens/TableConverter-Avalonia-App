using Avalonia.Controls;
using TableConverter.Components.Extensions.Interfaces;

namespace TableConverter.Components.Extensions.Definitions
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
