using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TableConverter.DataModels;

namespace TableConverter.Services.ConverterHandlerServices
{
    internal class ConverterHandlerCsvInputService : ConverterHandlerInputAbstract
    {
        private string Delimiter = ",";
        private bool HasHeader = true;

        public override void InitializeControls()
        {
            Controls = new();

            var delimiter_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var delimiter_label = new Label()
            {
                Content = "Delimiter:",
            };

            var delimiter_text_box = new TextBox()
            {
                Text = Delimiter,
            };

            delimiter_text_box.TextChanged += (sender, e) =>
            {
                if (sender is TextBox text_box && !string.IsNullOrEmpty(text_box.Text))
                {
                    text_box.Text = Delimiter = text_box.Text!.Substring(0, 1);
                }
            };

            delimiter_stack_panel.Children.Add(delimiter_label);
            delimiter_stack_panel.Children.Add(delimiter_text_box);

            Controls?.Add(delimiter_stack_panel);

            var has_header_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            var has_header_check_box = new CheckBox()
            {
                IsChecked = HasHeader
            };

            has_header_check_box.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox check_box)
                {
                    HasHeader = check_box.IsChecked ?? false;
                }
            };

            var has_header_label = new Label()
            {
                Content = "Has Header",
                VerticalAlignment = VerticalAlignment.Center
            };

            has_header_stack_panel.Children.Add(has_header_check_box);
            has_header_stack_panel.Children.Add(has_header_label);

            Controls?.Add(has_header_stack_panel);
        }

        public override Task<TableData> ReadTextAsync(string text)
        {
            return Task.Run(() =>
            {
                var headers = new List<string>();
                var rows = new List<string[]>();

                using (var csv_reader = new CsvHelper.CsvReader(new StringReader(text), new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = Delimiter,
                    NewLine = Environment.NewLine,
                    BadDataFound = null,
                }))
                {
                    foreach (var row in csv_reader.GetRecords<dynamic>())
                    {
                        if (HasHeader && headers.Count == 0)
                        {
                            headers.AddRange(((IDictionary<string, object>)row).Keys.ToList());
                        }
                        else if (!HasHeader && headers.Count == 0)
                        {
                            for (long i = 0; i < ((IDictionary<string, object>)row).Keys.LongCount(); i++)
                            {
                                headers.Add($"Column {i}");
                            }
                        }

                        rows.Add(((IDictionary<string, object>)row).Select(x => x.Value?.ToString() ?? string.Empty).ToArray());
                    }
                }

                return new TableData(headers, rows);
            }); 
        }
    }
}
