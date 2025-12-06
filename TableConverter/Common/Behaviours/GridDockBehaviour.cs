using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;

namespace TableConverter.Common.Behaviours;

public class GridDockBehavior : AvaloniaObject
{
    public static readonly AttachedProperty<Dock> OrientationProperty =
        AvaloniaProperty.RegisterAttached<GridDockBehavior, Grid, Dock>(
            "Orientation", Dock.Left);

    public static void SetOrientation(Grid grid, Dock value) =>
        grid.SetValue(OrientationProperty, value);

    public static Dock GetOrientation(Grid grid) =>
        grid.GetValue(OrientationProperty);
    
    public static readonly AttachedProperty<string?> DefinitionsProperty =
        AvaloniaProperty.RegisterAttached<GridDockBehavior, Grid, string?>(
            "Definitions");

    public static void SetDefinitions(Grid grid, string? value) =>
        grid.SetValue(DefinitionsProperty, value);

    public static string? GetDefinitions(Grid grid) =>
        grid.GetValue(DefinitionsProperty);
    
    public static readonly AttachedProperty<int?> PositionProperty =
        AvaloniaProperty.RegisterAttached<GridDockBehavior, Control, int?>(
            "Position");

    public static void SetPosition(Control control, int? value) =>
        control.SetValue(PositionProperty, value);

    public static int? GetPosition(Control control) =>
        control.GetValue(PositionProperty);

    static GridDockBehavior()
    {
        OrientationProperty.Changed.AddClassHandler<Grid>(OnPropertyChanged);
        DefinitionsProperty.Changed.AddClassHandler<Grid>(OnPropertyChanged);
    }

    private static void OnPropertyChanged(Grid grid, AvaloniaPropertyChangedEventArgs e)
    {
        AttachCollectionChanged(grid);
        UpdateLayout(grid);
    }

    private static readonly HashSet<Grid> SubscribedGrids = [];

    private static void AttachCollectionChanged(Grid grid)
    {
        if (grid.Children is not INotifyCollectionChanged notify) 
            return;
        
        if (SubscribedGrids.Contains(grid)) 
            return;
        
        notify.CollectionChanged += 
            (_, _) => UpdateLayout(grid);
        
        SubscribedGrids.Add(grid);
    }

    private static void UpdateLayout(Grid grid)
    {
        var defs = (GetDefinitions(grid) ?? string.Join(",", Enumerable.Repeat("*", grid.Children.Count)))
            .Split(',');

        var orientation = GetOrientation(grid);
        var isColumnLayout = orientation is Dock.Left or Dock.Right;
        
        if (isColumnLayout)
        {
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            
            foreach (var def in defs)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition(ParseGridLength(def)));
            }
        }
        else
        {
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            
            foreach (var def in defs)
            {
                grid.RowDefinitions.Add(new RowDefinition(ParseGridLength(def)));
            }
        }
        
        for (var i = 0; i < grid.Children.Count; i++)
        {
            var child = grid.Children[i];
            
            var pos = GetPosition(child) ?? i;
            
            pos = Math.Min(pos, defs.Length - 1);

            if (isColumnLayout)
            {
                Grid.SetColumn(child, pos);
                Grid.SetRow(child, 0);
            }
            else
            {
                Grid.SetRow(child, pos);
                Grid.SetColumn(child, 0);
            }
        }
    }

    private static GridLength ParseGridLength(string s)
    {
        s = s.Trim();
        
        if (s.EndsWith('*'))
        {
            return double.TryParse(s.TrimEnd('*'), out var v) 
                ? new GridLength(v, GridUnitType.Star) 
                : new GridLength(1, GridUnitType.Star);
        }

        return double.TryParse(s, out var val) 
            ? new GridLength(val, GridUnitType.Pixel) 
            : new GridLength(1, GridUnitType.Auto);
    }
}