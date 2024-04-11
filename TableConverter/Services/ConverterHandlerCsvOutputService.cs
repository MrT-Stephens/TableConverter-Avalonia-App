using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services
{
    public class ConverterHandlerCsvOutputService : ConverterHandlerOutputAbstract
    {
        private string Delimiter = ",";
        private bool HasHeader = true;

        public override void InitializeControls()
        {
            var delimiter_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var delimiter_label = new Label()
            {
                Content = "Delimiter:",
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            var delimiter_text_box = new TextBox()
            {
                Text = Delimiter,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
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
                Content = "Print Header",
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
                VerticalAlignment = VerticalAlignment.Center
            };

            has_header_stack_panel.Children.Add(has_header_check_box);
            has_header_stack_panel.Children.Add(has_header_label);

            Controls?.Add(has_header_stack_panel);
        }

        public override Task<string> ConvertAsync(string[] headers, string[][] rows, object? progress_bar)
        {
            return Task.Run(() =>
            {
                using (var writer = new StringWriter())
                using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = Delimiter,
                    NewLine = Environment.NewLine,
                    HasHeaderRecord = HasHeader
                }))
                {
                    List<object> records = new List<object>();

                    for (long i = 0; i < rows.LongLength; i++)
                    {
                        dynamic record = new ExpandoObject();

                        for (int j = 0; j < headers.Length; j++)
                        {
                            ((IDictionary<string, object>)record)[headers[j]] = rows[i][j];
                        }

                        records.Add(record);

                        SetProgressBarValue(progress_bar, i, 0, rows.LongLength);
                    }

                    csv.WriteRecords(records);

                    return writer.ToString();
                }
            });
        }
    }
}
