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

        var jsonLinesFormatTypeStackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var jsonLinesFormatTypeLabel = new TextBlock
        {
            Text = "JSONLines File Format:"
        };

        var jsonLinesFormatTypeComboBox = new ComboBox
        {
            ItemsSource = Options!.JsonLinesFormatTypes,
            SelectedItem = Options!.SelectedJsonLinesFormatType,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        jsonLinesFormatTypeComboBox.SelectionChanged += (sender, e) =>
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is string selectedJsonLinesFormatType)
                Options!.SelectedJsonLinesFormatType = selectedJsonLinesFormatType;
        };

        jsonLinesFormatTypeStackPanel.Children.Add(jsonLinesFormatTypeLabel);
        jsonLinesFormatTypeStackPanel.Children.Add(jsonLinesFormatTypeComboBox);

        Controls?.Add(jsonLinesFormatTypeStackPanel);
    }
}