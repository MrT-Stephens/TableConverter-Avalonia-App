using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Layout;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlersWithControls;

public class ConverterHandlerExcelOutputWithControls : ConverterHandlerExcelOutput, IInitializeControls
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
            Text = "Excel Workbook Sheet Name:"
        };

        var delimiterTextBox = new TextBox
        {
            Text = Options!.SheetName
        };

        delimiterTextBox.TextChanged += (sender, e) =>
        {
            if (sender is TextBox textBox) Options!.SheetName = textBox.Text ?? "Sheet1";
        };

        delimiterStackPanel.Children.Add(delimiterLabel);
        delimiterStackPanel.Children.Add(delimiterTextBox);

        Controls?.Add(delimiterStackPanel);
    }
}