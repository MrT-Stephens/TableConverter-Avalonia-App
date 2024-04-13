using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using CsvHelper.Configuration;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services
{
    internal class ConverterHandlerXmlOutputService : ConverterHandlerOutputAbstract
    {
        private string XmlRootNodeName { get; set; } = "root";

        private string XmlElementNodeName { get; set; } = "element";

        private bool MinifyXml { get; set; } = false;

        public override void InitializeControls()
        {
            var xml_root_node_name_text_box = new TextBox
            {
                Text = XmlRootNodeName
            };
        }

        public override Task<string> ConvertAsync(string[] headers, string[][] rows, object? progress_bar)
        {
            return Task.Run(() =>
            {
                var string_builder = new StringBuilder();

                return string_builder.ToString();
            });
        }
    }
}
