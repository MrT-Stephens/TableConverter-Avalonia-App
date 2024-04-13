﻿using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Text;
using System.Threading.Tasks;
using TableConverter.Interfaces;
using System.Xml;
using Avalonia.Threading;
using System.IO;

namespace TableConverter.Services
{
    internal class ConverterHandlerXmlOutputService : ConverterHandlerOutputAbstract
    {
        private string XmlRootNodeName { get; set; } = "root";

        private string XmlElementNodeName { get; set; } = "element";

        private bool MinifyXml { get; set; } = false;

        public override void InitializeControls()
        {
            var xml_root_node_name_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var xml_root_node_name_label = new Label()
            {
                Content = "XML Root Node Name:",
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            var xml_root_node_name_text_box = new TextBox()
            {
                Text = XmlElementNodeName,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            xml_root_node_name_text_box.TextChanged += (sender, e) =>
            {
                if (sender is TextBox text_box && !string.IsNullOrEmpty(text_box.Text))
                {
                    XmlElementNodeName = text_box.Text ?? "root";
                }
            };

            xml_root_node_name_stack_panel.Children.Add(xml_root_node_name_label);
            xml_root_node_name_stack_panel.Children.Add(xml_root_node_name_text_box);

            Controls?.Add(xml_root_node_name_stack_panel);

            var xml_element_node_name_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var xml_element_node_name_label = new Label()
            {
                Content = "XML Element Node Name:",
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            var xml_element_node_name_text_box = new TextBox()
            {
                Text = XmlRootNodeName,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            xml_element_node_name_text_box.TextChanged += (sender, e) =>
            {
                if (sender is TextBox text_box && !string.IsNullOrEmpty(text_box.Text))
                {
                    XmlRootNodeName = text_box.Text ?? "element";
                }
            };

            xml_element_node_name_stack_panel.Children.Add(xml_element_node_name_label);
            xml_element_node_name_stack_panel.Children.Add(xml_element_node_name_text_box);

            Controls?.Add(xml_element_node_name_stack_panel);

            var minify_xml_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            var minify_xml_check_box = new CheckBox()
            {
                IsChecked = MinifyXml
            };

            minify_xml_check_box.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox check_box)
                {
                    MinifyXml = check_box.IsChecked ?? false;
                }
            };

            var minify_xml_label = new Label()
            {
                Content = "Minify XML",
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
                VerticalAlignment = VerticalAlignment.Center
            };

            minify_xml_stack_panel.Children.Add(minify_xml_check_box);
            minify_xml_stack_panel.Children.Add(minify_xml_label);

            Controls?.Add(minify_xml_stack_panel);
        }

        public override Task<string> ConvertAsync(string[] headers, string[][] rows, object? progress_bar)
        {
            return Task.Run(() =>
            {
                XmlDocument xml_document = new XmlDocument();

                // Create XML declaration
                XmlDeclaration xml_declaration = xml_document.CreateXmlDeclaration("1.0", "UTF-8", null);
                xml_document.AppendChild(xml_declaration);

                // Create root element
                XmlElement root_element = xml_document.CreateElement(XmlRootNodeName.Replace(' ', '_'));
                xml_document.AppendChild(root_element);

                // Iterate over DataTable rows
                for (long i = 0; i < rows.LongLength; i++)
                {
                    // Create record element
                    XmlElement record_element = xml_document.CreateElement(XmlElementNodeName.Replace(' ', '_'));

                    // Iterate over DataTable columns
                    for (long j = 0; j < headers.LongLength; j++)
                    {
                        // Create element for each column and set its value
                        XmlElement columnElement = xml_document.CreateElement(headers[j].Replace(' ', '_'));
                        columnElement.InnerText = rows[i][j];
                        record_element.AppendChild(columnElement);
                    }

                    // Append record element to the root
                    root_element.AppendChild(record_element);

                    SetProgressBarValue(progress_bar, i, 0, rows.LongLength - 1);
                }

                StringWriter text_writer = new StringWriter();
                XmlTextWriter xml_writer = new XmlTextWriter(text_writer);

                xml_writer.Formatting = (MinifyXml) ? Formatting.None : Formatting.Indented;

                xml_document.WriteTo(xml_writer);

                return text_writer.ToString();
            });
        }
    }
}
