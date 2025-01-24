using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Layout;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlersWithControls;

public class ConverterHandlerSqlOutputWithControls : ConverterHandlerSQLOutput, IInitializeControls
{
    public Collection<Control> Controls { get; set; } = new();

    public void InitializeControls()
    {
        Controls.Clear();

        var tableNameStackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var tableNameLabel = new TextBlock
        {
            Text = "Table Name:"
        };

        var tableNameTextBox = new TextBox
        {
            Text = Options!.TableName
        };

        tableNameTextBox.TextChanged += (sender, e) =>
        {
            if (sender is TextBox textBox && !string.IsNullOrEmpty(textBox.Text)) Options!.TableName = textBox.Text;
        };

        tableNameStackPanel.Children.Add(tableNameLabel);
        tableNameStackPanel.Children.Add(tableNameTextBox);

        Controls?.Add(tableNameStackPanel);

        var quoteTypeStackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var quoteTypeLabel = new TextBlock
        {
            Text = "Quote Type:"
        };

        var quoteTypeComboBox = new ComboBox
        {
            ItemsSource = Options!.QuoteTypes.Keys,
            SelectedItem = Options!.SelectedQuoteType,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        quoteTypeComboBox.SelectionChanged += (sender, e) =>
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is string selectedQuoteType)
                Options!.SelectedQuoteType = selectedQuoteType;
        };

        quoteTypeStackPanel.Children.Add(quoteTypeLabel);
        quoteTypeStackPanel.Children.Add(quoteTypeComboBox);

        Controls?.Add(quoteTypeStackPanel);

        var insertMultiRowsAtOnceStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        var insertMultiRowsAtOnceCheckBox = new ToggleSwitch
        {
            IsChecked = Options!.InsertMultiRowsAtOnce
        };

        insertMultiRowsAtOnceCheckBox.IsCheckedChanged += (sender, e) =>
        {
            if (sender is ToggleSwitch checkBox) Options!.InsertMultiRowsAtOnce = checkBox.IsChecked ?? false;
        };

        var insertMultiRowsAtOnceLabel = new TextBlock
        {
            Text = "Insert Multiple Rows at Once"
        };

        insertMultiRowsAtOnceStackPanel.Children.Add(insertMultiRowsAtOnceCheckBox);
        insertMultiRowsAtOnceStackPanel.Children.Add(insertMultiRowsAtOnceLabel);

        Controls?.Add(insertMultiRowsAtOnceStackPanel);
    }
}