using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Layout;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlersWithControls;

public class ConverterHandlerJsonOutputWithControls : ConverterHandlerJsonOutput, IInitializeControls
{
    public Collection<Control> Controls { get; set; } = new();

    public void InitializeControls()
    {
        Controls.Clear();

        var json_format_type_stack_panel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var json_format_type_label = new TextBlock
        {
            Text = "JSON File Format:"
        };

        var json_format_type_combo_box = new ComboBox
        {
            ItemsSource = Options!.JsonFormatTypes,
            SelectedItem = Options!.SelectedJsonFormatType,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        json_format_type_combo_box.SelectionChanged += (sender, e) =>
        {
            if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_json_format_type)
                Options!.SelectedJsonFormatType = selected_json_format_type;
        };

        json_format_type_stack_panel.Children.Add(json_format_type_label);
        json_format_type_stack_panel.Children.Add(json_format_type_combo_box);

        Controls?.Add(json_format_type_stack_panel);

        var minify_json_stack_panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        var minify_json_check_box = new ToggleSwitch
        {
            IsChecked = Options!.MinifyJson
        };

        minify_json_check_box.IsCheckedChanged += (sender, e) =>
        {
            if (sender is ToggleSwitch check_box) Options!.MinifyJson = check_box.IsChecked ?? false;
        };

        var minify_json_label = new TextBlock
        {
            Text = "Minify JSON"
        };

        minify_json_stack_panel.Children.Add(minify_json_check_box);
        minify_json_stack_panel.Children.Add(minify_json_label);

        Controls?.Add(minify_json_stack_panel);
    }
}