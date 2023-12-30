using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Data;
using System.IO;
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

        public override Task<DataTable> ConvertAsync(IStorageFile input)
        {
            return Task.Run(async () =>
            {
                DataTable data_table = new DataTable();

                try
                {
                    // Xml reading is easy due to the in-built functions.
                    using var reader = new StreamReader(await input.OpenReadAsync());

                    DataSet data_set = new DataSet();

                    data_set.ReadXml(reader);

                    data_table = data_set.Tables[0];
                }
                catch (Exception)
                {
                    return new DataTable();
                }

                return data_table;
            });
        }

        public override Task<string> ConvertAsync(DataTable input, ProgressBar progress_bar)
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
                foreach (DataRow row in input.Rows)
                {
                    // Create record element
                    XmlElement record_element = xml_document.CreateElement((ElementName == string.Empty) ? "Element" : ElementName.Replace(' ', '_'));

                    // Iterate over DataTable columns
                    foreach (DataColumn column in input.Columns)
                    {
                        // Create element for each column and set its value
                        XmlElement columnElement = xml_document.CreateElement(column.ColumnName.Replace(' ', '_'));
                        columnElement.InnerText = row[column].ToString();
                        record_element.AppendChild(columnElement);
                    }

                    // Append record element to the root
                    root_element.AppendChild(record_element);

                    Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(input.Rows.IndexOf(row), 0, input.Rows.Count - 1, 0, 1000));
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