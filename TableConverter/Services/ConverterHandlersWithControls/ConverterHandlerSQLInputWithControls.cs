using Avalonia.Controls;
using Avalonia.Layout;
using System.Collections.ObjectModel;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlersWithControls
{
    public class ConverterHandlerSQLInputWithControls : ConverterHandlerSQLInput, IInitializeControls
    {
        public Collection<Control> Controls { get; set; } = new();

        public void InitializeControls()
        {
            Controls.Clear();

            var quote_type_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var quote_type_label = new TextBlock()
            {
                Text = "Quote Type:",
            };

            var quote_type_combo_box = new ComboBox()
            {
                ItemsSource = Options!.QuoteTypes.Keys,
                SelectedItem = Options!.SelectedQuoteType,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            quote_type_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_quote_type)
                {
                    Options!.SelectedQuoteType = selected_quote_type;
                }
            };

            quote_type_stack_panel.Children.Add(quote_type_label);
            quote_type_stack_panel.Children.Add(quote_type_combo_box);

            Controls?.Add(quote_type_stack_panel);

            var has_column_names_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            var has_column_names_check_box = new ToggleSwitch()
            {
                IsChecked = Options!.HasColumnNames
            };

            has_column_names_check_box.IsCheckedChanged += (sender, e) =>
            {
                if (sender is ToggleSwitch check_box)
                {
                    Options!.HasColumnNames = check_box.IsChecked ?? false;
                }
            };

            var has_column_names_label = new TextBlock()
            {
                Text = "Has Column Names",
            };

            has_column_names_stack_panel.Children.Add(has_column_names_check_box);
            has_column_names_stack_panel.Children.Add(has_column_names_label);

            Controls?.Add(has_column_names_stack_panel);
        }
    }
}
