using Avalonia.Controls;
using Avalonia.Layout;
using NPOI.OpenXml4Net.OPC;
using System;
using System.Collections.ObjectModel;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlersWithControls
{
    public class ConverterHandlerAsciiOutputWithControls : ConverterHandlerAsciiOutput, IInitializeControls
    {
        public Collection<Control> Controls { get; set; } = new();

        public void InitializeControls()
        {
            var table_type_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
            };

            var table_type_label = new TextBlock()
            {
                Text = "Table Type:",
            };

            var table_type_combo_box = new ComboBox()
            {
                ItemsSource = Options!.TableTypes.Keys,
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
                Orientation = Orientation.Vertical,
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

            var comment_type_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
            };

            var comment_type_label = new TextBlock()
            {
                Text = "Comment Type:",
            };

            var comment_type_combo_box = new ComboBox()
            {
                ItemsSource = Options!.CommentTypes.Keys,
                SelectedItem = Options!.SelectedCommentType,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            comment_type_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_comment_type)
                {
                    Options!.SelectedCommentType = selected_comment_type;
                }
            };

            comment_type_stack_panel.Children.Add(comment_type_label);
            comment_type_stack_panel.Children.Add(comment_type_combo_box);

            Controls?.Add(comment_type_stack_panel);

            var force_row_separators_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };

            var force_row_separators_check_box = new CheckBox()
            {
                IsChecked = Options!.ForceRowSeparators
            };

            force_row_separators_check_box.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox check_box)
                {
                    Options!.ForceRowSeparators = check_box.IsChecked ?? false;
                }
            };

            var force_row_separators_label = new TextBlock()
            {
                Text = "Force Row Separators",
            };

            force_row_separators_stack_panel.Children.Add(force_row_separators_check_box);
            force_row_separators_stack_panel.Children.Add(force_row_separators_label);

            Controls?.Add(force_row_separators_stack_panel);
        }
    }
}
