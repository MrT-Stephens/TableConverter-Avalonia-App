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

        var minifyHtmlStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        var minifyHtmlCheckBox = new ToggleSwitch
        {
            IsChecked = Options!.MinifyHtml
        };

        minifyHtmlCheckBox.IsCheckedChanged += (sender, e) =>
        {
            if (sender is ToggleSwitch checkBox) Options!.MinifyHtml = checkBox.IsChecked ?? false;
        };

        var minifyHtmlLabel = new TextBlock
        {
            Text = "Minify HTML"
        };

        minifyHtmlStackPanel.Children.Add(minifyHtmlCheckBox);
        minifyHtmlStackPanel.Children.Add(minifyHtmlLabel);

        Controls?.Add(minifyHtmlStackPanel);

        var includeTheadTbodyStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        var includeTheadTbodyCheckBox = new ToggleSwitch
        {
            IsChecked = Options!.IncludeTheadTbody
        };

        includeTheadTbodyCheckBox.IsCheckedChanged += (sender, e) =>
        {
            if (sender is ToggleSwitch checkBox) Options!.IncludeTheadTbody = checkBox.IsChecked ?? false;
        };

        var includeTheadTbodyLabel = new TextBlock
        {
            Text = "Include thead/tbody"
        };

        includeTheadTbodyStackPanel.Children.Add(includeTheadTbodyCheckBox);
        includeTheadTbodyStackPanel.Children.Add(includeTheadTbodyLabel);

        Controls?.Add(includeTheadTbodyStackPanel);
    }
}