using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TableConverter.Services.ConverterHandlers
{
    public class ConverterHandler_JSONLines : ConverterHandlerBase
    {
        private string[] JsonFormats { get; init; } =
        {
            "Objects",
            "Arrays"
        };

        private string CurrentTableType { get; set; } = "Objects";

        public override void InitializeControls()
        {
            base.InitializeControls();

            var JsonFormatStackPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };

            var JsonFormatLabel = new Label { Content = "JSONLines Format:" };

            var JsonFormatComboBox = new ComboBox
            {
                ItemsSource = JsonFormats,
                SelectedIndex = 0
            };

            JsonFormatComboBox.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox)
                {
                    CurrentTableType = ((ComboBox)sender).SelectedItem.ToString();
                }
            };

            JsonFormatStackPanel.Children.Add(JsonFormatLabel);
            JsonFormatStackPanel.Children.Add(JsonFormatComboBox);

            Controls?.Add(JsonFormatStackPanel);
        }

        public override Task<(List<string>, List<string[]>)> ConvertAsync(IStorageFile input)
        {
            return Task.Run(async () =>
            {
                List<string> column_values = new List<string>();
                List<string[]> row_values = new List<string[]>();

                try
                {
                    bool first_line = true;

                    using var reader = new StreamReader(await input.OpenReadAsync());

                    while (!reader.EndOfStream)
                    {
                        string line = await reader.ReadLineAsync();

                        if (line.StartsWith('{') && line.EndsWith('}'))
                        {
                            List<string> row = new List<string>();
                            string[] strings = line.Trim('{', '}').Split(',');

                            foreach (string str in strings)
                            {
                                string[] key_value = str.Split(':');

                                if (first_line)
                                {
                                    column_values.Add(key_value[0].Trim('"'));
                                }

                                row.Add(key_value[1].Trim('"'));
                            }

                            row_values.Add(row.ToArray());
                        }
                        else
                        {
                            string[] strings = line.Trim('[', ']').Split(',');

                            if (first_line)
                            {
                                foreach (string str in strings)
                                {
                                    column_values.Add(str.Trim('"'));
                                }
                            }
                            else
                            {
                                row_values.Add(strings.Select(str => str.Trim('"')).ToArray());
                            }
                        }

                        first_line = false;
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
                string output = string.Empty;

                switch (CurrentTableType)
                {
                    case "Objects":
                        {
                            for (int i = 0; i < row_values.Length; ++i)
                            {
                                output += "{";

                                for (int j = 0; j < column_values.Length; ++j)
                                {
                                    output += $"\"{column_values[j]}\":\"{row_values[i][j]}\"";

                                    if (j != row_values.Length - 1)
                                    {
                                        output += ",";
                                    }
                                }

                                output += "}";

                                if (i != row_values.Length - 1)
                                {
                                    output += Environment.NewLine;
                                }

                                Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, row_values.Length - 1, 0, 100));
                            }

                            break;
                        }
                    case "Arrays":
                        {
                            output += "[" + string.Join(",", column_values.Select(column => $"\"{column}\"").ToArray()) + "]" + Environment.NewLine;

                            for (int i = 0; i < row_values.Length; ++i)
                            {
                                output += "[";

                                output += string.Join(",", row_values[i].Select(str => $"\"{str}\"").ToArray());

                                output += "]";

                                if (i != row_values.Length - 1)
                                {
                                    output += Environment.NewLine;
                                }

                                Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, row_values.Length - 1, 0, 100));
                            }

                            break;
                        }
                }

                return output;
            });
        }
    }
}
