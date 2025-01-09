using System;
using Avalonia;
using Avalonia.Controls;

namespace TableConverter.Components.Extensions;

public class SpacedUniformGrid : Panel
{
    public static readonly StyledProperty<int> RowsProperty =
        AvaloniaProperty.Register<SpacedUniformGrid, int>(nameof(Rows));

    public static readonly StyledProperty<int> ColumnsProperty =
        AvaloniaProperty.Register<SpacedUniformGrid, int>(nameof(Columns), 1);

    public static readonly StyledProperty<int> FirstColumnProperty =
        AvaloniaProperty.Register<SpacedUniformGrid, int>(nameof(FirstColumn));

    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<SpacedUniformGrid, double>(nameof(Spacing), 5);

    private int _columns;

    private int _rows;

    static SpacedUniformGrid()
    {
        AffectsMeasure<SpacedUniformGrid>(RowsProperty, ColumnsProperty, FirstColumnProperty, SpacingProperty);

        AffectsArrange<SpacedUniformGrid>(SpacingProperty);
    }

    public int Rows
    {
        get => GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    public int Columns
    {
        get => GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public int FirstColumn
    {
        get => GetValue(FirstColumnProperty);
        set => SetValue(FirstColumnProperty, value);
    }

    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        UpdateRowsAndColumns();

        var maxChildWidth = 0.0;
        var maxChildHeight = 0.0;

        Size childAvailableSize = new(
            (availableSize.Width - (Columns - 1) * Spacing) / Columns,
            double.PositiveInfinity // Let Avalonia figure out the height.
        );

        foreach (var child in Children)
        {
            if (!child.IsVisible) continue;

            child.Measure(childAvailableSize);

            maxChildWidth = Math.Max(maxChildWidth, child.DesiredSize.Width);
            maxChildHeight = Math.Max(maxChildHeight, child.DesiredSize.Height);
        }

        return new Size(
            (maxChildWidth + Spacing) * Columns - Spacing,
            (maxChildHeight + Spacing) * _rows - Spacing
        );
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var cellWidth = (finalSize.Width - (Columns - 1) * Spacing) / Columns;
        var cellHeight = (finalSize.Height - (_rows - 1) * Spacing) / _rows;

        if (cellWidth < 0) cellWidth = 0;

        if (cellHeight < 0) cellHeight = 0;

        for (var i = 0; i < Children.Count; i++)
        {
            if (!Children[i].IsVisible) continue;

            Children[i].Arrange(new Rect(
                i % Columns * (cellWidth + Spacing),
                i / Columns * (cellHeight + Spacing),
                cellWidth,
                cellHeight
            ));
        }

        return finalSize;
    }


    private void UpdateRowsAndColumns()
    {
        _rows = Rows;
        _columns = Columns;

        if (FirstColumn >= Columns) SetCurrentValue(FirstColumnProperty, 0);

        var num = FirstColumn;

        foreach (var child in Children)
            if (child.IsVisible)
                num++;

        if (_rows == 0)
        {
            if (_columns == 0)
            {
                _rows = _columns = (int)Math.Ceiling(Math.Sqrt(num));
                return;
            }

            _rows = Math.DivRem(num, _columns, out var result);

            if (result != 0) _rows++;
        }
        else if (_columns == 0)
        {
            _columns = Math.DivRem(num, _rows, out var result2);

            if (result2 != 0) _columns++;
        }
    }
}