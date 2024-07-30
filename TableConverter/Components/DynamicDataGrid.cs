using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System;
using System.Collections.ObjectModel;

namespace TableConverter.Components
{
    public class DynamicDataGrid : DataGrid
    {
        protected override Type StyleKeyOverride => typeof(DataGrid);

        public static readonly StyledProperty<ObservableCollection<string>> HeadersProperty =
            AvaloniaProperty.Register<DynamicDataGrid, ObservableCollection<string>>(nameof(Headers));

        public static readonly StyledProperty<ObservableCollection<string[]>> RowsProperty =
            AvaloniaProperty.Register<DynamicDataGrid, ObservableCollection<string[]>>(nameof(Rows));

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

        public DynamicDataGrid()
        {
            Headers = new();
            Rows = new();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            ItemsSource = Rows;

            UpdateColumns();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property.Name == nameof(HeadersProperty))
            {
                UpdateColumns();
            }
        }

        private void UpdateColumns()
        {
            Columns.Clear();

            for (int i = 0; i < Headers.Count; ++i)
            {
                Columns.Add(new DataGridTextColumn
                {
                    Header = new TextBox
                    {
                        [!TextBox.TextProperty] = new Binding()
                        {
                            Path = $"Headers[{i}]",
                            Source = this,
                            Mode = BindingMode.TwoWay,
                        },
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
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
    }
}
