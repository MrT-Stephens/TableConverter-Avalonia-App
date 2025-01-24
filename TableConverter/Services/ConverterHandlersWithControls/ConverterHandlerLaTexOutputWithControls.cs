using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Layout;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlersWithControls;

public class ConverterHandlerLaTexOutputWithControls : ConverterHandlerLaTexOutput, IInitializeControls
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
            Text = "LaTex Table Type:"
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
            ItemsSource = Options!.Alignments,
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

        var tableAlignmentStackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var tableAlignmentLabel = new TextBlock
        {
            Text = "Table Alignment:"
        };

        var tableAlignmentComboBox = new ComboBox
        {
            ItemsSource = Options!.Alignments,
            SelectedItem = Options!.SelectedTableAlignment,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        tableAlignmentComboBox.SelectionChanged += (sender, e) =>
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is string selectedTableAlignment)
                Options!.SelectedTableAlignment = selectedTableAlignment;
        };

        tableAlignmentStackPanel.Children.Add(tableAlignmentLabel);
        tableAlignmentStackPanel.Children.Add(tableAlignmentComboBox);

        Controls?.Add(tableAlignmentStackPanel);

        var captionAlignmentStackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var captionAlignmentLabel = new TextBlock
        {
            Text = "Caption Alignment:"
        };

        var captionAlignmentComboBox = new ComboBox
        {
            ItemsSource = Options!.CaptionAlignments,
            SelectedItem = Options!.SelectedCaptionAlignment,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        captionAlignmentComboBox.SelectionChanged += (sender, e) =>
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is string selectedCaptionAlignment)
                Options!.SelectedCaptionAlignment = selectedCaptionAlignment;
        };

        captionAlignmentStackPanel.Children.Add(captionAlignmentLabel);
        captionAlignmentStackPanel.Children.Add(captionAlignmentComboBox);

        Controls?.Add(captionAlignmentStackPanel);

        var captionNameStackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var captionNameLabel = new TextBlock
        {
            Text = "Caption Name:"
        };

        var captionNameTextBox = new TextBox
        {
            Text = Options!.CaptionName,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        captionNameTextBox.TextChanged += (sender, e) =>
        {
            if (sender is TextBox textBox) Options!.CaptionName = textBox.Text ?? "";
        };

        captionNameStackPanel.Children.Add(captionNameLabel);
        captionNameStackPanel.Children.Add(captionNameTextBox);

        Controls?.Add(captionNameStackPanel);

        var labelNameStackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var labelNameLabel = new TextBlock
        {
            Text = "Label Name:"
        };

        var labelNameTextBox = new TextBox
        {
            Text = Options!.LabelName,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        labelNameTextBox.TextChanged += (sender, e) =>
        {
            if (sender is TextBox textBox) Options!.LabelName = textBox.Text ?? "";
        };

        labelNameStackPanel.Children.Add(labelNameLabel);
        labelNameStackPanel.Children.Add(labelNameTextBox);

        Controls?.Add(labelNameStackPanel);

        var minimalWorkingExampleStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        var minimalWorkingExampleCheckBox = new ToggleSwitch
        {
            IsChecked = Options!.MinimalWorkingExample
        };

        minimalWorkingExampleCheckBox.IsCheckedChanged += (sender, e) =>
        {
            if (sender is ToggleSwitch checkBox) Options!.MinimalWorkingExample = checkBox.IsChecked ?? false;
        };

        var minimalWorkingExampleLabel = new TextBlock
        {
            Text = "Minimal Working Example"
        };

        minimalWorkingExampleStackPanel.Children.Add(minimalWorkingExampleCheckBox);
        minimalWorkingExampleStackPanel.Children.Add(minimalWorkingExampleLabel);

        Controls?.Add(minimalWorkingExampleStackPanel);

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