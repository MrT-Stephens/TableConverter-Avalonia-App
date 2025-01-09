using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlersWithControls;

public class ConverterHandlerPdfOutputWithControls : ConverterHandlerPdfOutput, IInitializeControls
{
    public Collection<Control> Controls { get; set; } = new();

    public void InitializeControls()
    {
        Controls.Clear();

        var ColourDataTemplate = new FuncDataTemplate<string>((item, _) =>
        {
            var stack_panel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var square = new Rectangle
            {
                Width = 15,
                Height = 15,
                Fill = Brush.Parse(item),
                Margin = new Thickness(0, 0, 10, 0)
            };

            var text_block = new TextBlock
            {
                Text = item
            };

            stack_panel.Children.Add(square);
            stack_panel.Children.Add(text_block);

            return stack_panel;
        });

        var background_colour_stack_panel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var background_colour_label = new TextBlock
        {
            Text = "Background Colour:"
        };

        var background_colour_combo_box = new ComboBox
        {
            ItemsSource = Options!.Colours,
            SelectedItem = Options!.SelectedBackgroundColor,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        background_colour_combo_box.DataTemplates.Add(ColourDataTemplate);

        background_colour_combo_box.SelectionChanged += (sender, e) =>
        {
            if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_background_colour)
                Options!.SelectedBackgroundColor = selected_background_colour;
        };

        background_colour_stack_panel.Children.Add(background_colour_label);
        background_colour_stack_panel.Children.Add(background_colour_combo_box);

        Controls?.Add(background_colour_stack_panel);

        var foreground_colour_stack_panel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var foreground_colour_label = new TextBlock
        {
            Text = "Text Colour:"
        };

        var foreground_colour_combo_box = new ComboBox
        {
            ItemsSource = Options!.Colours,
            SelectedItem = Options!.SelectedForegroundColor,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        foreground_colour_combo_box.DataTemplates.Add(ColourDataTemplate);

        foreground_colour_combo_box.SelectionChanged += (sender, e) =>
        {
            if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_foreground_colour)
                Options!.SelectedForegroundColor = selected_foreground_colour;
        };

        foreground_colour_stack_panel.Children.Add(foreground_colour_label);
        foreground_colour_stack_panel.Children.Add(foreground_colour_combo_box);

        Controls?.Add(foreground_colour_stack_panel);

        var bold_header_stack_panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        var bold_header_check_box = new ToggleSwitch
        {
            IsChecked = Options!.BoldHeader
        };

        bold_header_check_box.IsCheckedChanged += (sender, e) =>
        {
            if (sender is ToggleSwitch check_box) Options!.BoldHeader = check_box.IsChecked ?? false;
        };

        var bold_header_label = new TextBlock
        {
            Text = "Bold Header"
        };

        bold_header_stack_panel.Children.Add(bold_header_check_box);
        bold_header_stack_panel.Children.Add(bold_header_label);

        Controls?.Add(bold_header_stack_panel);

        var show_grid_lines_stack_panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        var show_grid_lines_check_box = new ToggleSwitch
        {
            IsChecked = Options!.ShowGridLines
        };

        show_grid_lines_check_box.IsCheckedChanged += (sender, e) =>
        {
            if (sender is ToggleSwitch check_box) Options!.ShowGridLines = check_box.IsChecked ?? false;
        };

        var show_grid_lines_label = new TextBlock
        {
            Text = "Show Grid Lines"
        };

        show_grid_lines_stack_panel.Children.Add(show_grid_lines_check_box);
        show_grid_lines_stack_panel.Children.Add(show_grid_lines_label);

        Controls?.Add(show_grid_lines_stack_panel);
    }
}