using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Layout;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlersWithControls;

public class ConverterHandlerMultiLineOutputWithControls : ConverterHandlerMultiLineOutput, IInitializeControls
{
    public Collection<Control> Controls { get; set; } = new();

    public void InitializeControls()
    {
        Controls.Clear();

        var rowSeparatorStackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var rowSeparatorLabel = new TextBlock
        {
            Text = "Row Separator:"
        };

        var rowSeparatorTextBox = new TextBox
        {
            Text = Options!.RowSeparator
        };

        rowSeparatorTextBox.TextChanged += (sender, e) =>
        {
            if (sender is TextBox textBox) Options!.RowSeparator = textBox.Text ?? "";
        };

        rowSeparatorStackPanel.Children.Add(rowSeparatorLabel);
        rowSeparatorStackPanel.Children.Add(rowSeparatorTextBox);

        Controls?.Add(rowSeparatorStackPanel);
    }
}