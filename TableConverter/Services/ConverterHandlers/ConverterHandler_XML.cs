using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace TableConverter.Services.ConverterHandlers
{
    public class ConverterHandler_XML : ConverterHandlerBase
    {
        public string RootName { get; set; } = "";
        public string ElementName { get; set; } = "";
        public bool MinifyXml { get; set; } = false;

        public override void InitializeControls()
        {
            base.InitializeControls();

            var RootNameStackPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };

            var RootNameLabel = new Label { Content = "Root name:" };

            var RootNameTextBox = new TextBox
            {
                Text = RootName
            };

            RootNameTextBox.TextChanged += (sender, e) =>
            {
                if (sender is TextBox)
                {
                    RootName = ((TextBox)sender).Text;
                }
            };

            RootNameStackPanel.Children.Add(RootNameLabel);
            RootNameStackPanel.Children.Add(RootNameTextBox);

            var ElementNameStackPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };

            var ElementNameLabel = new Label { Content = "Element name:" };

            var ElementNameTextBox = new TextBox
            {
                Text = ElementName
            };

            ElementNameTextBox.TextChanged += (sender, e) =>
            {
                if (sender is TextBox)
                {
                    ElementName = ((TextBox)sender).Text;
                }
            };

            ElementNameStackPanel.Children.Add(ElementNameLabel);
            ElementNameStackPanel.Children.Add(ElementNameTextBox);

            var MinifyXmlCheckBox = new CheckBox
            {
                Content = "Minify XML"
            };

            MinifyXmlCheckBox.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox)
                {
                    MinifyXml = ((CheckBox)sender).IsChecked.Value;
                }
            };

            Controls?.Add(RootNameStackPanel);
            Controls?.Add(ElementNameStackPanel);
            Controls?.Add(MinifyXmlCheckBox);
        }

        public override Task<(List<string>, List<string[]>)> ConvertAsync(IStorageFile input)
        {
            return Task.Run(async () =>
            {
                List<string> column_values = new List<string>();
                List<string[]> row_values = new List<string[]>();

                try
                {
                    // Xml reading is easy due to the in-built functions.
                    using var reader = new StreamReader(await input.OpenReadAsync());

                    DataSet data_set = new DataSet();

                    data_set.ReadXml(reader);

                    for (int i = 0; i < data_set.Tables.Count; i++)
                    {
                        foreach (DataColumn column in data_set.Tables[i].Columns)
                        {
                            column_values.Add(column.ColumnName);
                        }

                        foreach (DataRow row in data_set.Tables[i].Rows)
                        {
                            List<string> row_value = new List<string>();

                            foreach (DataColumn column in data_set.Tables[i].Columns)
                            {
                                row_value.Add(row[column].ToString());
                            }

                            row_values.Add(row_value.ToArray());
                        }
                    }
                }
                catch (Exception)
                {
                    return (new List<string>(), new List<string[]>());
                }

                return (column_values, row_values);
            });
        }

        public override Task<string> ConvertAsync(string[] column_values, string[][] row_values, ProgressBar progress_bar)
        {
            return Task.Run(() =>
            {
                XmlDocument xml_document = new XmlDocument();

                // Create XML declaration
                XmlDeclaration xml_declaration = xml_document.CreateXmlDeclaration("1.0", "UTF-8", null);
                xml_document.AppendChild(xml_declaration);

                // Create root element
                XmlElement root_element = xml_document.CreateElement((RootName == string.Empty) ? "Root" : RootName.Replace(' ', '_'));
                xml_document.AppendChild(root_element);

                // Iterate over DataTable rows
                for (int i = 0; i < row_values.Length; ++i)
                {
                    // Create record element
                    XmlElement record_element = xml_document.CreateElement((ElementName == string.Empty) ? "Element" : ElementName.Replace(' ', '_'));

                    // Iterate over DataTable columns
                    for (int j = 0; j < column_values.Length; ++j)
                    {
                        // Create element for each column and set its value
                        XmlElement columnElement = xml_document.CreateElement(column_values[j].Replace(' ', '_'));
                        columnElement.InnerText = row_values[i][j].ToString();
                        record_element.AppendChild(columnElement);
                    }

                    // Append record element to the root
                    root_element.AppendChild(record_element);

                    Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, row_values.Length - 1, 0, 100));
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