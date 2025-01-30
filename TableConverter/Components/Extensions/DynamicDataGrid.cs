using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;

namespace TableConverter.Components.Extensions;

public class DynamicDataGrid : DataGrid
{
    public static readonly StyledProperty<ObservableCollection<string>> HeadersProperty =
        AvaloniaProperty.Register<DynamicDataGrid, ObservableCollection<string>>(nameof(Headers));

    public static readonly StyledProperty<ObservableCollection<string[]>> RowsProperty =
        AvaloniaProperty.Register<DynamicDataGrid, ObservableCollection<string[]>>(nameof(Rows));

    public DynamicDataGrid()
    {
        Headers = [];
        Rows = [];
    }

    protected override Type StyleKeyOverride => typeof(DataGrid);

    public ObservableCollection<string> Headers
    {
        get => GetValue(HeadersProperty);
        set => SetValue(HeadersProperty, value);
    }

    public ObservableCollection<string[]> Rows
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

        if (Headers is null)
            return;

        for (var i = 0; i < Headers.Count; ++i)
            Columns.Add(new DataGridTextColumn
            {
                Header = new TextBox
                {
                    [!TextBox.TextProperty] = new Binding
                    {
                        Path = $"Headers[{i}]",
                        Source = this,
                        Mode = BindingMode.TwoWay
                    },
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                },
                Binding = new Binding($"[{i}]"),
                CanUserSort = false,
                IsReadOnly = false,
                CanUserReorder = false,
                CanUserResize = true,
                DisplayIndex = i
            });
    }
}