using Avalonia.Controls;
using Avalonia.Layout;
using System.Collections.ObjectModel;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlersWithControls
{
    public class ConverterHandlerSQLOutputWithControls : ConverterHandlerSQLOutput, IInitializeControls
    {
        public Collection<Control> Controls { get; set; } = new();

        public void InitializeControls()
        {
            Controls.Clear();

            var table_name_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var table_name_label = new TextBlock()
            {
                Text = "Table Name:",
            };

            var table_name_text_box = new TextBox()
            {
                Text = Options!.TableName,
            };

            table_name_text_box.TextChanged += (sender, e) =>
            {
                if (sender is TextBox text_box && !string.IsNullOrEmpty(text_box.Text))
                {
                    Options!.TableName = text_box.Text;
                }
            };

            table_name_stack_panel.Children.Add(table_name_label);
            table_name_stack_panel.Children.Add(table_name_text_box);

            Controls?.Add(table_name_stack_panel);

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

            var insert_multi_rows_at_once_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            var insert_multi_rows_at_once_check_box = new ToggleSwitch()
            {
                IsChecked = Options!.InsertMultiRowsAtOnce
            };

            insert_multi_rows_at_once_check_box.IsCheckedChanged += (sender, e) =>
            {
                if (sender is ToggleSwitch check_box)
                {
                    Options!.InsertMultiRowsAtOnce = check_box.IsChecked ?? false;
                }
            };

            var insert_multi_rows_at_once_label = new TextBlock()
            {
                Text = "Insert Multiple Rows at Once",
            };

            insert_multi_rows_at_once_stack_panel.Children.Add(insert_multi_rows_at_once_check_box);
            insert_multi_rows_at_once_stack_panel.Children.Add(insert_multi_rows_at_once_label);

            Controls?.Add(insert_multi_rows_at_once_stack_panel);
        }
    }
}
