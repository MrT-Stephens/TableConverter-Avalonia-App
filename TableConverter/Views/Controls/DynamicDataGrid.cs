using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using SukiUI.Theme;
using TableConverter.Contracts;

namespace TableConverter.Views.Controls;

public class DynamicDataGrid : DataGrid
{
    public static readonly StyledProperty<ObservableCollection<string>> HeadersProperty =
        AvaloniaProperty.Register<DynamicDataGrid, ObservableCollection<string>>(nameof(Headers));

    public DynamicDataGrid()
    {
        Headers = [];
    }

    protected override Type StyleKeyOverride => typeof(DataGrid);

    public ObservableCollection<string> Headers
    {
        get => GetValue(HeadersProperty);
        set => SetValue(HeadersProperty, value);
    }
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        UpdateColumns();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        switch (change.Property.Name)
        {
            case nameof(Headers):
                UpdateColumns();
                break;
        }
    }

    private void UpdateColumns()
    {
        Columns.Clear();

        if (Headers.Count == 0)
            return;

        for (var i = 0; i < Headers.Count; i++)
        {
            // Cells are always rendered as text. A DataGridCheckBoxColumn variant used to live here
            // behind an `if (false)` guard, which made the branch unreachable dead code.
            var column = new DataGridTextColumn
            {
                Binding = new Binding
                {
                    Path = $"Item.Cells[{i}].Value",
                    Mode = BindingMode.TwoWay,
                },
            };

            var header = new TextBox
            {
                [!TextBox.TextProperty] = new Binding
                {
                    Path = $"Headers[{i}]",
                    Source = this,
                    Mode = BindingMode.TwoWay
                },
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            header.Classes.Add("Small");
            column.Header = header;
            column.CanUserSort = false;
            column.CanUserReorder = true;
            column.CanUserResize = true;
            column.DisplayIndex = i;
            column.IsReadOnly = false;

            Columns.Add(column);
        }
    }
}