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

        var xml_root_node_name_stack_panel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var xml_root_node_name_label = new TextBlock
        {
            Text = "XML Root Node Name:"
        };

        var xml_root_node_name_text_box = new TextBox
        {
            Text = Options!.XmlElementNodeName
        };

        xml_root_node_name_text_box.TextChanged += (sender, e) =>
        {
            if (sender is TextBox text_box && !string.IsNullOrEmpty(text_box.Text))
                Options!.XmlElementNodeName = text_box.Text ?? "root";
        };

        xml_root_node_name_stack_panel.Children.Add(xml_root_node_name_label);
        xml_root_node_name_stack_panel.Children.Add(xml_root_node_name_text_box);

        Controls?.Add(xml_root_node_name_stack_panel);

        var xml_element_node_name_stack_panel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var xml_element_node_name_label = new TextBlock
        {
            Text = "XML Element Node Name:"
        };

        var xml_element_node_name_text_box = new TextBox
        {
            Text = Options!.XmlRootNodeName
        };

        xml_element_node_name_text_box.TextChanged += (sender, e) =>
        {
            if (sender is TextBox text_box && !string.IsNullOrEmpty(text_box.Text))
                Options!.XmlRootNodeName = text_box.Text ?? "element";
        };

        xml_element_node_name_stack_panel.Children.Add(xml_element_node_name_label);
        xml_element_node_name_stack_panel.Children.Add(xml_element_node_name_text_box);

        Controls?.Add(xml_element_node_name_stack_panel);

        var minify_xml_stack_panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        var minify_xml_check_box = new ToggleSwitch
        {
            IsChecked = Options!.MinifyXml
        };

        minify_xml_check_box.IsCheckedChanged += (sender, e) =>
        {
            if (sender is ToggleSwitch check_box) Options!.MinifyXml = check_box.IsChecked ?? false;
        };

        var minify_xml_label = new TextBlock
        {
            Text = "Minify XML"
        };

        minify_xml_stack_panel.Children.Add(minify_xml_check_box);
        minify_xml_stack_panel.Children.Add(minify_xml_label);

        Controls?.Add(minify_xml_stack_panel);
    }
}