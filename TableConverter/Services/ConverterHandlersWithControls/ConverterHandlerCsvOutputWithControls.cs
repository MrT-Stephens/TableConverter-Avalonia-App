using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Layout;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlersWithControls;

public class ConverterHandlerCsvOutputWithControls : ConverterHandlerCsvOutput, IInitializeControls
{
    public Collection<Control> Controls { get; set; } = new();

    public void InitializeControls()
    {
        Controls.Clear();

        var delimiterStackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var delimiterLabel = new TextBlock
        {
            Text = "Delimiter:"
        };

        var delimiterTextBox = new TextBox
        {
            Text = Options!.Delimiter
        };

        delimiterTextBox.TextChanged += (sender, e) =>
        {
            if (sender is TextBox textBox && !string.IsNullOrEmpty(textBox.Text))
                textBox.Text = Options!.Delimiter = textBox.Text.Substring(0, 1);
        };

        delimiterStackPanel.Children.Add(delimiterLabel);
        delimiterStackPanel.Children.Add(delimiterTextBox);

        Controls?.Add(delimiterStackPanel);

        var hasHeaderStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        var hasHeaderCheckBox = new ToggleSwitch
        {
            IsChecked = Options!.Header
        };

        hasHeaderCheckBox.IsCheckedChanged += (sender, e) =>
        {
            if (sender is ToggleSwitch checkBox) Options!.Header = checkBox.IsChecked ?? false;
        };

        var hasHeaderLabel = new TextBlock
        {
            Text = "Print Header"
        };

        hasHeaderStackPanel.Children.Add(hasHeaderCheckBox);
        hasHeaderStackPanel.Children.Add(hasHeaderLabel);

        Controls?.Add(hasHeaderStackPanel);
    }
}