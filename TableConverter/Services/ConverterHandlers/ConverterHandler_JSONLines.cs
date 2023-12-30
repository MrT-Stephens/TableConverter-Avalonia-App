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

        public override Task<DataTable> ConvertAsync(IStorageFile input)
        {
            return Task.Run(async () =>
            {
                DataTable data_table = new DataTable();

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
                                    data_table.Columns.Add(key_value[0].Trim('"'));
                                }

                                row.Add(key_value[1].Trim('"'));
                            }

                            data_table.Rows.Add(row.ToArray());
                        }
                        else
                        {
                            string[] strings = line.Trim('[', ']').Split(',');

                            if (first_line)
                            {
                                foreach (string str in strings)
                                {
                                    data_table.Columns.Add(str.Trim('"'));
                                }
                            }
                            else
                            {
                                data_table.Rows.Add(strings.Select(str => str.Trim('"')).ToArray());
                            }
                        }

                        first_line = false;
                    }
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
                string output = string.Empty;

                switch (CurrentTableType)
                {
                    case "Objects":
                        {
                            for (int i = 0; i < input.Rows.Count; ++i)
                            {
                                output += "{";

                                foreach (DataColumn column in input.Columns)
                                {
                                    output += $"\"{column.ColumnName}\":\"{input.Rows[i][column.ColumnName]}\"";

                                    if (column.Ordinal != input.Columns.Count - 1)
                                    {
                                        output += ",";
                                    }
                                }

                                output += "}";

                                if (i != input.Rows.Count - 1)
                                {
                                    output += Environment.NewLine;
                                }

                                Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, input.Rows.Count - 1, 0, 1000));
                            }

                            break;
                        }
                    case "Arrays":
                        {
                            output += "[" + string.Join(",", input.Columns.Cast<DataColumn>().Select(column => $"\"{column.ColumnName}\"").ToArray()) + "]" + Environment.NewLine;

                            for (int i = 0; i < input.Rows.Count; ++i)
                            {
                                output += "[";

                                output += string.Join(",", input.Rows[i].ItemArray.Select(str => $"\"{str}\"").ToArray());

                                output += "]";

                                if (i != input.Rows.Count - 1)
                                {
                                    output += Environment.NewLine;
                                }

                                Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, input.Rows.Count - 1, 0, 1000));
                            }

                            break;
                        }
                }

                return output;
            });
        }
    }
}
