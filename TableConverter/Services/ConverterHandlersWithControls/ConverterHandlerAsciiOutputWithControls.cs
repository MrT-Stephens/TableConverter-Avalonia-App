using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Layout;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlersWithControls;

public class ConverterHandlerAsciiOutputWithControls : ConverterHandlerAsciiOutput, IInitializeControls
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
            ItemsSource = Options!.TableTypes.Keys,
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

        var commentTypeStackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var commentTypeLabel = new TextBlock
        {
            Text = "Comment Type:"
        };

        var commentTypeComboBox = new ComboBox
        {
            ItemsSource = Options!.CommentTypes.Keys,
            SelectedItem = Options!.SelectedCommentType,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        commentTypeComboBox.SelectionChanged += (sender, e) =>
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is string selectedCommentType)
                Options!.SelectedCommentType = selectedCommentType;
        };

        commentTypeStackPanel.Children.Add(commentTypeLabel);
        commentTypeStackPanel.Children.Add(commentTypeComboBox);

        Controls?.Add(commentTypeStackPanel);

        var forceRowSeparatorsStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var forceRowSeparatorsCheckBox = new CheckBox
        {
            IsChecked = Options!.ForceRowSeparators
        };

        forceRowSeparatorsCheckBox.IsCheckedChanged += (sender, e) =>
        {
            if (sender is CheckBox checkBox) Options!.ForceRowSeparators = checkBox.IsChecked ?? false;
        };

        var forceRowSeparatorsLabel = new TextBlock
        {
            Text = "Force Row Separators"
        };

        forceRowSeparatorsStackPanel.Children.Add(forceRowSeparatorsCheckBox);
        forceRowSeparatorsStackPanel.Children.Add(forceRowSeparatorsLabel);

        Controls?.Add(forceRowSeparatorsStackPanel);
    }
}