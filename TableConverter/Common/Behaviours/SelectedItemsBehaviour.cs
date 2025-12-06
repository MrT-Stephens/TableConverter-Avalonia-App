using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace TableConverter.Common.Behaviours;

public class SelectedItemsBehavior : AvaloniaObject
{
    #region Attached Properties

    public static readonly AttachedProperty<SelectedItemsCollection?> SelectedItemsProperty =
        AvaloniaProperty.RegisterAttached<SelectedItemsBehavior, Control, SelectedItemsCollection?>(
            "SelectedItems");

    public static void SetSelectedItems(AvaloniaObject element, SelectedItemsCollection? value) =>
        element.SetValue(SelectedItemsProperty, value);

    public static SelectedItemsCollection? GetSelectedItems(AvaloniaObject element) =>
        element.GetValue(SelectedItemsProperty);

    public static readonly AttachedProperty<bool> TrackRemovalProperty =
        AvaloniaProperty.RegisterAttached<SelectedItemsBehavior, Control, bool>(
            "TrackRemoval",
            defaultValue: true);

    public static void SetTrackRemoval(AvaloniaObject element, bool value) =>
        element.SetValue(TrackRemovalProperty, value);

    public static bool GetTrackRemoval(AvaloniaObject element) =>
        element.GetValue(TrackRemovalProperty);

    private static readonly AttachedProperty<bool> IsSubscribedProperty =
        AvaloniaProperty.RegisterAttached<SelectedItemsBehavior, Control, bool>(
            "IsSubscribed",
            defaultValue: false);

    #endregion

    #region Static Constructor

    static SelectedItemsBehavior()
    {
        SelectedItemsProperty.Changed.AddClassHandler<Control>(OnSelectedItemsChanged);
    }

    #endregion

    #region Subscription Logic

    private static void OnSelectedItemsChanged(Control control, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.OldValue != null)
            Unsubscribe(control);

        if (args.NewValue != null)
            Subscribe(control);
    }

    private static void Subscribe(Control control)
    {
        if (control.GetValue(IsSubscribedProperty))
            return;

        control.SetValue(IsSubscribedProperty, true);

        switch (control)
        {
            case SelectingItemsControl sic:
                sic.SelectionChanged += OnSelectingItemsControlSelectionChanged;
                break;

            case DataGrid dg:
                dg.AddHandler(DataGrid.SelectionChangedEvent, OnDataGridSelectionChanged);
                break;
        }

        // Initial sync
        var target = control.GetValue(SelectedItemsProperty);
        
        if (target != null)
        {
            var selectedItems = GetCurrentSelectedItems(control);
            
            foreach (var x in selectedItems)
                target.Add(x);
        }
    }

    private static void Unsubscribe(Control control)
    {
        if (!control.GetValue(IsSubscribedProperty))
            return;

        control.SetValue(IsSubscribedProperty, false);

        switch (control)
        {
            case SelectingItemsControl sic:
                sic.SelectionChanged -= OnSelectingItemsControlSelectionChanged;
                break;

            case DataGrid dg:
                dg.RemoveHandler(DataGrid.SelectionChangedEvent, OnDataGridSelectionChanged);
                break;
        }
    }

    #endregion

    #region Event Handlers

    private static void OnSelectingItemsControlSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is Control control)
            ApplySelectionDelta(control, e.AddedItems, e.RemovedItems);
    }

    private static void OnDataGridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is Control control)
            ApplySelectionDelta(control, e.AddedItems, e.RemovedItems);
    }

    private static void ApplySelectionDelta(Control control, IList added, IList removed)
    {
        var target = control.GetValue(SelectedItemsProperty);
        if (target == null)
            return;

        foreach (var a in added)
            target.Add(a);

        if (control.GetValue(TrackRemovalProperty))
            foreach (var r in removed)
                target.Remove(r);
    }

    #endregion

    #region Utility

    private static IEnumerable<object?> GetCurrentSelectedItems(Control c)
    {
        return c switch
        {
            SelectingItemsControl sic => Enumerable.Repeat(sic.SelectedItem, 1),
            DataGrid dg => dg.SelectedItems.Cast<object>(),
            _ => []
        };
    }

    #endregion
}
