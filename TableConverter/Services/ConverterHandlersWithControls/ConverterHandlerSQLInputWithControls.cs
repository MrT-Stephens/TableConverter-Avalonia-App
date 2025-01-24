using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Layout;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlersWithControls;

public class ConverterHandlerSqlInputWithControls : ConverterHandlerSQLInput, IInitializeControls
{
    public Collection<Control> Controls { get; set; } = new();

    public void InitializeControls()
    {
        Controls.Clear();

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

        var hasColumnNamesStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        var hasColumnNamesCheckBox = new ToggleSwitch
        {
            IsChecked = Options!.HasColumnNames
        };

        hasColumnNamesCheckBox.IsCheckedChanged += (sender, e) =>
        {
            if (sender is ToggleSwitch checkBox) Options!.HasColumnNames = checkBox.IsChecked ?? false;
        };

        var hasColumnNamesLabel = new TextBlock
        {
            Text = "Has Column Names"
        };

        hasColumnNamesStackPanel.Children.Add(hasColumnNamesCheckBox);
        hasColumnNamesStackPanel.Children.Add(hasColumnNamesLabel);

        Controls?.Add(hasColumnNamesStackPanel);
    }
}