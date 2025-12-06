using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using SukiUI.Theme;
using TableConverter.Contracts;
using TableConverter.Converters;

namespace TableConverter.Views.Controls;

public class DynamicDataGrid : DataGrid
{
    public static readonly StyledProperty<ObservableTableData?> TableDataProperty =
        AvaloniaProperty.Register<DynamicDataGrid, ObservableTableData?>(nameof(TableData));

    public DynamicDataGrid()
    {
        TableData = new ObservableTableData();
    }

    protected override Type StyleKeyOverride => typeof(DataGrid);

    public ObservableTableData? TableData
    {
        get => GetValue(TableDataProperty);
        set => SetValue(TableDataProperty, value);
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
            case nameof(TableData.ObservableColumns):
                UpdateColumns();
                break;
            case nameof(TableData.ObservableRows):
                UpdateRows();
                break;
        }
    }

    private void UpdateRows()
    {
        ItemsSource = TableData?.ObservableRows;
    }

    private void UpdateColumns()
    {
        Columns.Clear();

        if (TableData is null || TableData.ColumnCount == 0)
            return;

        for (var i = 0; i < TableData.ColumnCount; i++)
        {
            var tableDataColumn = TableData.Columns[i];
            DataGridBoundColumn column;

            if (tableDataColumn.DataType == typeof(bool))
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
                    Path = $"[{i}].Name",
                    Source = TableData.ObservableColumns,
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
}