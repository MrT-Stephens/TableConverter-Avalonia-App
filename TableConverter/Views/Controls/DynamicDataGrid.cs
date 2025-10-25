using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using TableConverter.Converters;

namespace TableConverter.Views.Controls;

public class DynamicDataGrid : DataGrid
{
    public static readonly StyledProperty<ObservableCollection<string>?> HeadersProperty =
        AvaloniaProperty.Register<DynamicDataGrid, ObservableCollection<string>?>(nameof(Headers));

    public static readonly StyledProperty<ObservableCollection<object[]>?> RowsProperty =
        AvaloniaProperty.Register<DynamicDataGrid, ObservableCollection<object[]>?>(nameof(Rows));

    public DynamicDataGrid()
    {
        Headers = [];
        Rows = [];
    }

    protected override Type StyleKeyOverride => typeof(DataGrid);

    public ObservableCollection<string>? Headers
    {
        get => GetValue(HeadersProperty);
        set => SetValue(HeadersProperty, value);
    }

    public ObservableCollection<object[]>? Rows
    {
        get => GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        UpdateRows();
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
            case nameof(Rows):
                UpdateRows();
                break;
        }
    }

    private void UpdateRows()
    {
        ItemsSource = Rows;
    }

    private void UpdateColumns()
    {
        Columns.Clear();

        if (Headers is null || Headers.Count == 0)
            return;

        for (var i = 0; i < Headers.Count; ++i)
        {
            DataGridBoundColumn column;

            if (Rows is { Count: > 0 } && Rows[0].Length > i && Rows[0][i] is bool)
            {
                column = new DataGridCheckBoxColumn
                {
                    Binding = new Binding
                    {
                        Path = $"[{i}]",
                        Mode = BindingMode.TwoWay,
                        Converter = new FuncValueConverter<bool>(Convert.ToBoolean),
                    }
                };
            }
            else
            {
                column = new DataGridTextColumn
                {
                    Binding = new Binding
                    {
                        Path = $"[{i}]",
                        Mode = BindingMode.TwoWay,
                        Converter = new FuncValueConverter<string>(x => x?.ToString() ?? string.Empty),
                    },
                };
            }

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
            column.CanUserSort = true;
            column.CanUserReorder = true;
            column.CanUserResize = true;
            column.DisplayIndex = i;
            column.IsReadOnly = false;

            Columns.Add(column);
        }
    }

    private void OnSort(object? sender, DataGridColumnEventArgs e)
    {
        e.Handled = true;
    }
}