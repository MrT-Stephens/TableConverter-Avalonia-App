using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace TableConverter.Components.Extensions;

public class GridListBox : ListBox
{
    public static readonly StyledProperty<int> ColumnsProperty =
        AvaloniaProperty.Register<GridListBox, int>(nameof(Columns), 2);
    
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<GridListBox, double>(nameof(Spacing), 0.0);

    public GridListBox()
    {
        SelectionMode = SelectionMode.AlwaysSelected | SelectionMode.Single;
        ItemsPanel = new FuncTemplate<Panel?>(() =>
        {
            var panel = new SpacedUniformGrid
            {
                Columns = Columns,
                Spacing = Spacing,
            };

            return panel;
        });
    }

    protected override Type StyleKeyOverride => typeof(ListBox);

    public int Columns
    {
        get => GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }
    
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }
}