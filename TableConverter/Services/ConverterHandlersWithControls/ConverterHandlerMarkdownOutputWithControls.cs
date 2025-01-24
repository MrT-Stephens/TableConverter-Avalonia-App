using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Layout;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlersWithControls;

public class ConverterHandlerMarkdownOutputWithControls : ConverterHandlerMarkdownOutput, IInitializeControls
{
    public Collection<Control> Controls { get; set; } = new();

    public void InitializeControls()
    {
        Controls.Clear();

        var tableTypeStackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var tableTypeLabel = new TextBlock
        {
            Text = "Table Type:"
        };

        var tableTypeComboBox = new ComboBox
        {
            ItemsSource = Options!.TableTypes,
            SelectedItem = Options!.SelectedTableType,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        tableTypeComboBox.SelectionChanged += (sender, e) =>
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is string selectedTableType)
                Options!.SelectedTableType = selectedTableType;
        };

        tableTypeStackPanel.Children.Add(tableTypeLabel);
        tableTypeStackPanel.Children.Add(tableTypeComboBox);

        Controls?.Add(tableTypeStackPanel);

        var textAlignmentStackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var textAlignmentLabel = new TextBlock
        {
            Text = "Text Alignment:"
        };

        var textAlignmentComboBox = new ComboBox
        {
            ItemsSource = Options!.TextAlignment.Keys,
            SelectedItem = Options!.SelectedTextAlignment,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        textAlignmentComboBox.SelectionChanged += (sender, e) =>
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is string selectedTextAlignment)
                Options!.SelectedTextAlignment = selectedTextAlignment;
        };

        textAlignmentStackPanel.Children.Add(textAlignmentLabel);
        textAlignmentStackPanel.Children.Add(textAlignmentComboBox);

        Controls?.Add(textAlignmentStackPanel);

        var boldColumnNamesStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        var boldColumnNamesCheckBox = new ToggleSwitch
        {
            IsChecked = Options!.BoldColumnNames
        };

        boldColumnNamesCheckBox.IsCheckedChanged += (sender, e) =>
        {
            if (sender is ToggleSwitch checkBox) Options!.BoldColumnNames = checkBox.IsChecked ?? false;
        };

        var boldColumnNamesLabel = new TextBlock
        {
            Text = "Bold Column Names"
        };

        boldColumnNamesStackPanel.Children.Add(boldColumnNamesCheckBox);
        boldColumnNamesStackPanel.Children.Add(boldColumnNamesLabel);

        Controls?.Add(boldColumnNamesStackPanel);

        var boldFirstColumnStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        var boldFirstColumnCheckBox = new ToggleSwitch
        {
            IsChecked = Options!.BoldFirstColumn
        };

        boldFirstColumnCheckBox.IsCheckedChanged += (sender, e) =>
        {
            if (sender is ToggleSwitch checkBox) Options!.BoldFirstColumn = checkBox.IsChecked ?? false;
        };

        var boldFirstColumnLabel = new TextBlock
        {
            Text = "Bold First Column"
        };

        boldFirstColumnStackPanel.Children.Add(boldFirstColumnCheckBox);
        boldFirstColumnStackPanel.Children.Add(boldFirstColumnLabel);

        Controls?.Add(boldFirstColumnStackPanel);
    }
}