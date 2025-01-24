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

        var colourDataTemplate = new FuncDataTemplate<string>((item, _) =>
        {
            var stackPanel = new StackPanel
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

            var textBlock = new TextBlock
            {
                Text = item
            };

            stackPanel.Children.Add(square);
            stackPanel.Children.Add(textBlock);

            return stackPanel;
        });

        var backgroundColourStackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var backgroundColourLabel = new TextBlock
        {
            Text = "Background Colour:"
        };

        var backgroundColourComboBox = new ComboBox
        {
            ItemsSource = Options!.Colours,
            SelectedItem = Options!.SelectedBackgroundColor,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        backgroundColourComboBox.DataTemplates.Add(colourDataTemplate);

        backgroundColourComboBox.SelectionChanged += (sender, e) =>
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is string selectedBackgroundColour)
                Options!.SelectedBackgroundColor = selectedBackgroundColour;
        };

        backgroundColourStackPanel.Children.Add(backgroundColourLabel);
        backgroundColourStackPanel.Children.Add(backgroundColourComboBox);

        Controls?.Add(backgroundColourStackPanel);

        var foregroundColourStackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var foregroundColourLabel = new TextBlock
        {
            Text = "Text Colour:"
        };

        var foregroundColourComboBox = new ComboBox
        {
            ItemsSource = Options!.Colours,
            SelectedItem = Options!.SelectedForegroundColor,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        foregroundColourComboBox.DataTemplates.Add(colourDataTemplate);

        foregroundColourComboBox.SelectionChanged += (sender, e) =>
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is string selectedForegroundColour)
                Options!.SelectedForegroundColor = selectedForegroundColour;
        };

        foregroundColourStackPanel.Children.Add(foregroundColourLabel);
        foregroundColourStackPanel.Children.Add(foregroundColourComboBox);

        Controls?.Add(foregroundColourStackPanel);

        var boldHeaderStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        var boldHeaderCheckBox = new ToggleSwitch
        {
            IsChecked = Options!.BoldHeader
        };

        boldHeaderCheckBox.IsCheckedChanged += (sender, e) =>
        {
            if (sender is ToggleSwitch checkBox) Options!.BoldHeader = checkBox.IsChecked ?? false;
        };

        var boldHeaderLabel = new TextBlock
        {
            Text = "Bold Header"
        };

        boldHeaderStackPanel.Children.Add(boldHeaderCheckBox);
        boldHeaderStackPanel.Children.Add(boldHeaderLabel);

        Controls?.Add(boldHeaderStackPanel);

        var showGridLinesStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        var showGridLinesCheckBox = new ToggleSwitch
        {
            IsChecked = Options!.ShowGridLines
        };

        showGridLinesCheckBox.IsCheckedChanged += (sender, e) =>
        {
            if (sender is ToggleSwitch checkBox) Options!.ShowGridLines = checkBox.IsChecked ?? false;
        };

        var showGridLinesLabel = new TextBlock
        {
            Text = "Show Grid Lines"
        };

        showGridLinesStackPanel.Children.Add(showGridLinesCheckBox);
        showGridLinesStackPanel.Children.Add(showGridLinesLabel);

        Controls?.Add(showGridLinesStackPanel);
    }
}