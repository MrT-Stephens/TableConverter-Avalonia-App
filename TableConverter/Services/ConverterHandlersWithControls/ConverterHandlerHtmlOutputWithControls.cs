using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Layout;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlersWithControls;

public class ConverterHandlerHtmlOutputWithControls : ConverterHandlerHtmlOutput, IInitializeControls
{
    public Collection<Control> Controls { get; set; } = new();

    public void InitializeControls()
    {
        Controls.Clear();

        var minify_html_stack_panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        var minify_html_check_box = new ToggleSwitch
        {
            IsChecked = Options!.MinifyHtml
        };

        minify_html_check_box.IsCheckedChanged += (sender, e) =>
        {
            if (sender is ToggleSwitch check_box) Options!.MinifyHtml = check_box.IsChecked ?? false;
        };

        var minify_html_label = new TextBlock
        {
            Text = "Minify HTML"
        };

        minify_html_stack_panel.Children.Add(minify_html_check_box);
        minify_html_stack_panel.Children.Add(minify_html_label);

        Controls?.Add(minify_html_stack_panel);

        var include_thead_tbody_stack_panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        var include_thead_tbody_check_box = new ToggleSwitch
        {
            IsChecked = Options!.IncludeTheadTbody
        };

        include_thead_tbody_check_box.IsCheckedChanged += (sender, e) =>
        {
            if (sender is ToggleSwitch check_box) Options!.IncludeTheadTbody = check_box.IsChecked ?? false;
        };

        var include_thead_tbody_label = new TextBlock
        {
            Text = "Include thead/tbody"
        };

        include_thead_tbody_stack_panel.Children.Add(include_thead_tbody_check_box);
        include_thead_tbody_stack_panel.Children.Add(include_thead_tbody_label);

        Controls?.Add(include_thead_tbody_stack_panel);
    }
}