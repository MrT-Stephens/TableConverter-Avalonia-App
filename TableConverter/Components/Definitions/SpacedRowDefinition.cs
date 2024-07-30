using Avalonia.Controls;
using TableConverter.Components.Interfaces;

namespace TableConverter.Components.Definitions
{
    public class SpacedRowDefinition : RowDefinition, ISpacing
    {
        public double Spacing
        {
            get => Height.Value;
            set => Height = new GridLength(value, GridUnitType.Pixel);
        }

        public SpacedRowDefinition(double height) : base(height, GridUnitType.Pixel)
        { 
        }
    }
}
