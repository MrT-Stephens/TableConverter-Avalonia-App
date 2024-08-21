using Avalonia.Controls;
using Avalonia.Layout;
using System.Collections.ObjectModel;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlersWithControls
{
    public class ConverterHandlerMarkdownOutputWithControls : ConverterHandlerMarkdownOutput, IInitializeControls
    {
        public Collection<Control> Controls { get; set; } = new();

        public void InitializeControls()
        {
            var table_type_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var table_type_label = new TextBlock()
            {
                Text = "Table Type:",
            };

            var table_type_combo_box = new ComboBox()
            {
                ItemsSource = Options!.TableTypes,
                SelectedItem = Options!.SelectedTableType,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            table_type_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_table_type)
                {
                    Options!.SelectedTableType = selected_table_type;
                }
            };

            table_type_stack_panel.Children.Add(table_type_label);
            table_type_stack_panel.Children.Add(table_type_combo_box);

            Controls?.Add(table_type_stack_panel);

            var text_alignment_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var text_alignment_label = new TextBlock()
            {
                Text = "Text Alignment:",
            };

            var text_alignment_combo_box = new ComboBox()
            {
                ItemsSource = Options!.TextAlignment.Keys,
                SelectedItem = Options!.SelectedTextAlignment,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            text_alignment_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_text_alignment)
                {
                    Options!.SelectedTextAlignment = selected_text_alignment;
                }
            };

            text_alignment_stack_panel.Children.Add(text_alignment_label);
            text_alignment_stack_panel.Children.Add(text_alignment_combo_box);

            Controls?.Add(text_alignment_stack_panel);

            var bold_column_names_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            var bold_column_names_check_box = new ToggleSwitch()
            {
                IsChecked = Options!.BoldColumnNames
            };

            bold_column_names_check_box.IsCheckedChanged += (sender, e) =>
            {
                if (sender is ToggleSwitch check_box)
                {
                    Options!.BoldColumnNames = check_box.IsChecked ?? false;
                }
            };

            var bold_column_names_label = new TextBlock()
            {
                Text = "Bold Column Names",
            };

            bold_column_names_stack_panel.Children.Add(bold_column_names_check_box);
            bold_column_names_stack_panel.Children.Add(bold_column_names_label);

            Controls?.Add(bold_column_names_stack_panel);

            var bold_first_column_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            var bold_first_column_check_box = new ToggleSwitch()
            {
                IsChecked = Options!.BoldFirstColumn
            };

            bold_first_column_check_box.IsCheckedChanged += (sender, e) =>
            {
                if (sender is ToggleSwitch check_box)
                {
                    Options!.BoldFirstColumn = check_box.IsChecked ?? false;
                }
            };

            var bold_first_column_label = new TextBlock()
            {
                Text = "Bold First Column",
            };

            bold_first_column_stack_panel.Children.Add(bold_first_column_check_box);
            bold_first_column_stack_panel.Children.Add(bold_first_column_label);

            Controls?.Add(bold_first_column_stack_panel);
        }
    }
}
