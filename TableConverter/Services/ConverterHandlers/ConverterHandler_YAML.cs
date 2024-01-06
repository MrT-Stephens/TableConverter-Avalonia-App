using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TableConverter.Services.ConverterHandlers
{
    public class ConverterHandler_YAML : ConverterHandlerBase
    {
        private string[] Quotes { get; init; } = new string[]
        {
            "No Quotes",
            "Single Quotes",
            "Double Quotes"
        };

        private string CurrentQuote { get; set; } = "No Quotes";

        public override void InitializeControls()
        {
            base.InitializeControls();

            var QuoteStackPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };

            var QuoteLabel = new Label { Content = "Quote Type:" };

            var QuoteComboBox = new ComboBox
            {
                ItemsSource = Quotes,
                SelectedIndex = 0
            };

            QuoteComboBox.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox)
                {
                    CurrentQuote = ((ComboBox)sender).SelectedItem.ToString();
                }
            };

            QuoteStackPanel.Children.Add(QuoteLabel);
            QuoteStackPanel.Children.Add(QuoteComboBox);

            Controls?.Add(QuoteStackPanel);
        }

        public override Task<(List<string>, List<string[]>)> ConvertAsync(IStorageFile input)
        {
            return Task.Run(async () =>
            {
                List<string> column_values = new List<string>();
                List<string[]> row_values = new List<string[]>();

                try
                {
                    using var reader = new StreamReader(await input.OpenReadAsync());

                    Dictionary<string, string> yaml_data = null;

                    bool first_line = true;
                    while (!reader.EndOfStream)
                    {
                        string line = await reader.ReadLineAsync();

                        if (line.StartsWith("---"))
                        {
                            continue;
                        }
                        if (line.StartsWith('-') && line.EndsWith('-'))
                        {
                            if (yaml_data != null)
                            {
                                if (first_line)
                                {
                                    column_values = yaml_data.Select(values => values.Key).ToList();
                                    first_line = false;
                                }

                                row_values.Add(yaml_data.Select(values => values.Value).ToArray());
                            }

                            yaml_data = new Dictionary<string, string>();
                        }
                        else
                        {
                            string[] line_data = line.Split(':');

                            if (line_data.Length == 2)
                            {
                                yaml_data.Add(line_data[0].Trim(), line_data[1].Trim());
                            }
                        }
                    }

                    row_values.Add(yaml_data.Select(values => values.Value).ToArray());
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
                string output = $"---{Environment.NewLine}";

                var quote = CurrentQuote switch
                {
                    "No Quotes" => "",
                    "Single Quotes" => "'",
                    "Double Quotes" => "\"",
                    _ => ""
                };

                for (int i = 0; i < row_values.Length; i++)
                {
                    output += $"-{Environment.NewLine}";

                    for (int j = 0; j < column_values.Length; j++)
                    {
                        output += $"\t{column_values[j].Replace(' ', '_')}: {quote}{row_values[i][j]}{quote}{Environment.NewLine}";
                    }

                    Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, row_values.Length - 1, 0, 1000));
                }

                return output;
            });
        }
    }
}
