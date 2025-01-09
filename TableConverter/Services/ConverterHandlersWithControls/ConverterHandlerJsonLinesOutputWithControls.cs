using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Layout;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlersWithControls;

public class ConverterHandlerJsonLinesOutputWithControls : ConverterHandlerJsonLinesOutput, IInitializeControls
{
    public Collection<Control> Controls { get; set; } = new();

    public void InitializeControls()
    {
        Controls.Clear();

        var json_lines_format_type_stack_panel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var json_lines_format_type_label = new TextBlock
        {
            Text = "JSONLines File Format:"
        };

        var json_lines_format_type_combo_box = new ComboBox
        {
            ItemsSource = Options!.JsonLinesFormatTypes,
            SelectedItem = Options!.SelectedJsonLinesFormatType,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        json_lines_format_type_combo_box.SelectionChanged += (sender, e) =>
        {
            if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_json_lines_format_type)
                Options!.SelectedJsonLinesFormatType = selected_json_lines_format_type;
        };

        json_lines_format_type_stack_panel.Children.Add(json_lines_format_type_label);
        json_lines_format_type_stack_panel.Children.Add(json_lines_format_type_combo_box);

        Controls?.Add(json_lines_format_type_stack_panel);
    }
}