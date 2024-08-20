using Avalonia.Controls;
using Avalonia.Layout;
using System.Collections.ObjectModel;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlersWithControls
{
    public class ConverterHandlerCsvOutputWithControls : ConverterHandlerCsvOutput, IInitializeControls
    {
        public Collection<Control> Controls { get; set; } = new();

        public void InitializeControls()
        {
            var delimiter_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var delimiter_label = new TextBlock()
            {
                Text = "Delimiter:",
            };

            var delimiter_text_box = new TextBox()
            {
                Text = Options!.Delimiter,
            };

            delimiter_text_box.TextChanged += (sender, e) =>
            {
                if (sender is TextBox text_box && !string.IsNullOrEmpty(text_box.Text))
                {
                    text_box.Text = Options!.Delimiter = text_box.Text.Substring(0, 1);
                }
            };

            delimiter_stack_panel.Children.Add(delimiter_label);
            delimiter_stack_panel.Children.Add(delimiter_text_box);

            Controls?.Add(delimiter_stack_panel);

            var has_header_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            var has_header_check_box = new ToggleSwitch()
            {
                IsChecked = Options!.Header
            };

            has_header_check_box.IsCheckedChanged += (sender, e) =>
            {
                if (sender is ToggleSwitch check_box)
                {
                    Options!.Header = check_box.IsChecked ?? false;
                }
            };

            var has_header_label = new TextBlock()
            {
                Text = "Print Header",
            };

            has_header_stack_panel.Children.Add(has_header_check_box);
            has_header_stack_panel.Children.Add(has_header_label);

            Controls?.Add(has_header_stack_panel);
        }
    }
}
