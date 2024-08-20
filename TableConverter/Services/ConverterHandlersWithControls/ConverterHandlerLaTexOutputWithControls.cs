using Avalonia.Controls;
using Avalonia.Layout;
using System.Collections.ObjectModel;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlersWithControls
{
    public class ConverterHandlerLaTexOutputWithControls : ConverterHandlerLaTexOutput, IInitializeControls
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
                Text = "LaTex Table Type:",
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
                ItemsSource = Options!.Alignments,
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

            var table_alignment_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var table_alignment_label = new TextBlock()
            {
                Text = "Table Alignment:",
            };

            var table_alignment_combo_box = new ComboBox()
            {
                ItemsSource = Options!.Alignments,
                SelectedItem = Options!.SelectedTableAlignment,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            table_alignment_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_table_alignment)
                {
                    Options!.SelectedTableAlignment = selected_table_alignment;
                }
            };

            table_alignment_stack_panel.Children.Add(table_alignment_label);
            table_alignment_stack_panel.Children.Add(table_alignment_combo_box);

            Controls?.Add(table_alignment_stack_panel);

            var caption_alignment_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var caption_alignment_label = new TextBlock()
            {
                Text = "Caption Alignment:",
            };

            var caption_alignment_combo_box = new ComboBox()
            {
                ItemsSource = Options!.CaptionAlignments,
                SelectedItem = Options!.SelectedCaptionAlignment,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            caption_alignment_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_caption_alignment)
                {
                    Options!.SelectedCaptionAlignment = selected_caption_alignment;
                }
            };

            caption_alignment_stack_panel.Children.Add(caption_alignment_label);
            caption_alignment_stack_panel.Children.Add(caption_alignment_combo_box);

            Controls?.Add(caption_alignment_stack_panel);

            var caption_name_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var caption_name_label = new TextBlock()
            {
                Text = "Caption Name:",
            };

            var caption_name_text_box = new TextBox()
            {
                Text = Options!.CaptionName,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            caption_name_text_box.TextChanged += (sender, e) =>
            {
                if (sender is TextBox text_box)
                {
                    Options!.CaptionName = text_box.Text ?? "";
                }
            };

            caption_name_stack_panel.Children.Add(caption_name_label);
            caption_name_stack_panel.Children.Add(caption_name_text_box);

            Controls?.Add(caption_name_stack_panel);

            var label_name_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var label_name_label = new TextBlock()
            {
                Text = "Label Name:",
            };

            var label_name_text_box = new TextBox()
            {
                Text = Options!.LabelName,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            label_name_text_box.TextChanged += (sender, e) =>
            {
                if (sender is TextBox text_box)
                {
                    Options!.LabelName = text_box.Text ?? "";
                }
            };

            label_name_stack_panel.Children.Add(label_name_label);
            label_name_stack_panel.Children.Add(label_name_text_box);

            Controls?.Add(label_name_stack_panel);

            var minimal_working_example_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            var minimal_working_example_check_box = new ToggleSwitch()
            {
                IsChecked = Options!.MinimalWorkingExample
            };

            minimal_working_example_check_box.IsCheckedChanged += (sender, e) =>
            {
                if (sender is ToggleSwitch check_box)
                {
                    Options!.MinimalWorkingExample = check_box.IsChecked ?? false;
                }
            };

            var minimal_working_example_label = new TextBlock()
            {
                Text = "Minimal Working Example",
            };

            minimal_working_example_stack_panel.Children.Add(minimal_working_example_check_box);
            minimal_working_example_stack_panel.Children.Add(minimal_working_example_label);

            Controls?.Add(minimal_working_example_stack_panel);

            var bold_header_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            var bold_header_check_box = new ToggleSwitch()
            {
                IsChecked = Options!.BoldHeader
            };

            bold_header_check_box.IsCheckedChanged += (sender, e) =>
            {
                if (sender is ToggleSwitch check_box)
                {
                    Options!.BoldHeader = check_box.IsChecked ?? false;
                }
            };

            var bold_header_label = new TextBlock()
            {
                Text = "Bold Header",
            };

            bold_header_stack_panel.Children.Add(bold_header_check_box);
            bold_header_stack_panel.Children.Add(bold_header_label);

            Controls?.Add(bold_header_stack_panel);

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
