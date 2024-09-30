using Avalonia.Controls;
using Avalonia.Layout;
using System.Collections.ObjectModel;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlersWithControls
{
    public class ConverterHandlerJsonInputWithControls : ConverterHandlerJsonInput, IInitializeControls
    {
        public Collection<Control> Controls { get; set; } = new();

        public void InitializeControls()
        {
            Controls.Clear();

            var json_format_type_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var json_format_type_label = new TextBlock()
            {
                Text = "JSON File Format:",
            };

            var json_format_type_combo_box = new ComboBox()
            {
                ItemsSource = Options!.JsonFormatTypes,
                SelectedItem = Options!.SelectedJsonFormatType,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            json_format_type_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_json_format_type)
                {
                    Options!.SelectedJsonFormatType = selected_json_format_type;
                }
            };

            json_format_type_stack_panel.Children.Add(json_format_type_label);
            json_format_type_stack_panel.Children.Add(json_format_type_combo_box);

            Controls?.Add(json_format_type_stack_panel);
        }
    }
}
