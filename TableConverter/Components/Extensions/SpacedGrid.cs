using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using TableConverter.Components.Extensions.Definitions;
using TableConverter.Components.Extensions.Interfaces;

namespace TableConverter.Components.Extensions
{
    public class SpacedGrid : Grid
    {
        public static readonly StyledProperty<double> RowSpacingProperty =
            AvaloniaProperty.Register<SpacedGrid, double>(nameof(RowSpacing), 5);

        public static readonly StyledProperty<double> ColumnSpacingProperty =
            AvaloniaProperty.Register<SpacedGrid, double>(nameof(ColumnSpacing), 5);

        public double RowSpacing
        {
            get => GetValue(RowSpacingProperty);
            set => SetValue(RowSpacingProperty, value);
        }

        public double ColumnSpacing
        {
            get => GetValue(ColumnSpacingProperty);
            set => SetValue(ColumnSpacingProperty, value);
        }

        public IEnumerable<RowDefinition> UserDefinedRowDefinitions =>
            RowDefinitions.Where(definition => definition is not ISpacing);

        public IEnumerable<ColumnDefinition> UserDefinedColumnDefinitions =>
            ColumnDefinitions.Where(definition => definition is not ISpacing);

        public SpacedGrid()
        {
            Children.CollectionChanged += ChildrenCollectionChanged!;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            RowDefinitions.CollectionChanged += delegate { UpdateSpacedRows(); };
            ColumnDefinitions.CollectionChanged += delegate { UpdateSpacedColumns(); };

            UpdateSpacedRows();
            UpdateSpacedColumns();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            switch (change.Property.Name)
            {
                case nameof(RowSpacing):
                    RecalculateRowSpacing();
                    break;

                case nameof(ColumnSpacing):
                    RecalculateColumnSpacing();
                    break;
            }
        }
        private void ChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if ((e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace) && e.NewItems != null)
            {
                foreach (Control item in e.NewItems)
                {
                    item.Initialized += ItemInitialized!;
                }
            }
        }

        private void ItemInitialized(object sender, EventArgs e)
        {
            if (sender is Control item)
            {
                item.Initialized -= ItemInitialized!;

                SetRow(item, GetRow(item) * 2); // 1 -> 2 or 2 -> 4
                SetRowSpan(item, GetRowSpan(item) * 2 - 1); // 2 -> 3 or 3 -> 5

                SetColumn(item, GetColumn(item) * 2); // 1 -> 2 or 2 -> 4
                SetColumnSpan(item, GetColumnSpan(item) * 2 - 1); // 2 -> 3 or 3 -> 5
            }
        }

        private void UpdateSpacedRows()
        {
            var userRowDefinitions = UserDefinedRowDefinitions.ToList(); // User-defined rows (e.g. the ones defined in XAML files)
            var actualRowDefinitions = new RowDefinitions(); // User-defined + spacing rows

            int currentUserDefinition = 0,
                currentActualDefinition = 0;

            while (currentUserDefinition < userRowDefinitions.Count)
            {
                if (currentActualDefinition % 2 == 0) // Even rows are user-defined rows (0, 2, 4, 6, 8, 10, ...)
                {
                    actualRowDefinitions.Add(userRowDefinitions[currentUserDefinition]);
                    currentUserDefinition++;
                }
                else  // Odd rows are spacing rows (1, 3, 5, 7, 9, 11, ...)
                {
                    actualRowDefinitions.Add(new SpacedRowDefinition(RowSpacing));
                }

                currentActualDefinition++;
            }

            RowDefinitions = actualRowDefinitions;
            RowDefinitions.CollectionChanged += delegate { UpdateSpacedRows(); };
        }

        private void UpdateSpacedColumns()
        {
            var userColumnDefinitions = UserDefinedColumnDefinitions.ToList(); // User-defined columns (e.g. the ones defined in XAML files)
            var actualColumnDefinitions = new ColumnDefinitions(); // User-defined + spacing columns

            int currentUserDefinition = 0,
                currentActualDefinition = 0;

            while (currentUserDefinition < userColumnDefinitions.Count)
            {
                if (currentActualDefinition % 2 == 0) // Even columns are user-defined columns (0, 2, 4, 6, 8, 10, ...)
                {
                    actualColumnDefinitions.Add(userColumnDefinitions[currentUserDefinition]);
                    currentUserDefinition++;
                }
                else // Odd columns are spacing columns (1, 3, 5, 7, 9, 11, ...)
                {
                    actualColumnDefinitions.Add(new SpacedColumnDefinition(ColumnSpacing));
                }

                currentActualDefinition++;
            }

            ColumnDefinitions = actualColumnDefinitions;
            ColumnDefinitions.CollectionChanged += delegate { UpdateSpacedColumns(); };
        }

        private void RecalculateRowSpacing() =>
            RowDefinitions.OfType<ISpacing>().Select(spacingRow => spacingRow.Spacing = RowSpacing);

        private void RecalculateColumnSpacing() =>
            ColumnDefinitions.OfType<ISpacing>().Select(spacingColumn => spacingColumn.Spacing = ColumnSpacing);
    }
}