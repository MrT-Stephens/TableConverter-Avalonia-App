using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Layout;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlersWithControls;

public class ConverterHandlerXmlOutputWithControls : ConverterHandlerXmlOutput, IInitializeControls
{
    public Collection<Control> Controls { get; set; } = new();

    public void InitializeControls()
    {
        Controls.Clear();

        var xmlRootNodeNameStackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var xmlRootNodeNameLabel = new TextBlock
        {
            Text = "XML Root Node Name:"
        };

        var xmlRootNodeNameTextBox = new TextBox
        {
            Text = Options!.XmlElementNodeName
        };

        xmlRootNodeNameTextBox.TextChanged += (sender, e) =>
        {
            if (sender is TextBox textBox && !string.IsNullOrEmpty(textBox.Text))
                Options!.XmlElementNodeName = textBox.Text ?? "root";
        };

        xmlRootNodeNameStackPanel.Children.Add(xmlRootNodeNameLabel);
        xmlRootNodeNameStackPanel.Children.Add(xmlRootNodeNameTextBox);

        Controls?.Add(xmlRootNodeNameStackPanel);

        var xmlElementNodeNameStackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var xmlElementNodeNameLabel = new TextBlock
        {
            Text = "XML Element Node Name:"
        };

        var xmlElementNodeNameTextBox = new TextBox
        {
            Text = Options!.XmlRootNodeName
        };

        xmlElementNodeNameTextBox.TextChanged += (sender, e) =>
        {
            if (sender is TextBox textBox && !string.IsNullOrEmpty(textBox.Text))
                Options!.XmlRootNodeName = textBox.Text ?? "element";
        };

        xmlElementNodeNameStackPanel.Children.Add(xmlElementNodeNameLabel);
        xmlElementNodeNameStackPanel.Children.Add(xmlElementNodeNameTextBox);

        Controls?.Add(xmlElementNodeNameStackPanel);

        var minifyXmlStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        var minifyXmlCheckBox = new ToggleSwitch
        {
            IsChecked = Options!.MinifyXml
        };

        minifyXmlCheckBox.IsCheckedChanged += (sender, e) =>
        {
            if (sender is ToggleSwitch checkBox) Options!.MinifyXml = checkBox.IsChecked ?? false;
        };

        var minifyXmlLabel = new TextBlock
        {
            Text = "Minify XML"
        };

        minifyXmlStackPanel.Children.Add(minifyXmlCheckBox);
        minifyXmlStackPanel.Children.Add(minifyXmlLabel);

        Controls?.Add(minifyXmlStackPanel);
    }
}