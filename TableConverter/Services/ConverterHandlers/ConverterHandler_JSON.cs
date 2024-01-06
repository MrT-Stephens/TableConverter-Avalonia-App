using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Avalonia.Threading;
using System.IO;

namespace TableConverter.Services.ConverterHandlers
{
    public class ConverterHandler_JSON : ConverterHandlerBase
    {
        private string[] TableTypes { get; init; } =
        {
            "Array of Objects",
            "2D Array",
            "Column Arrays",
            "Keyed Arrays"
        };

        private string CurrentTableType { get; set; } = "Array of Objects";
        private bool MinifyJson { get; set; } = false;

        public override void InitializeControls()
        {
            base.InitializeControls();

            var JsonFormatStackPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };

            var JsonFormatLabel = new Label { Content = "JSON Format:" };

            var JsonFormatComboBox = new ComboBox
            {
                ItemsSource = TableTypes,
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

            var MinifyJsonCheckBox = new CheckBox
            {
                Content = "Minify JSON",
                IsChecked = MinifyJson
            };

            MinifyJsonCheckBox.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox)
                {
                    MinifyJson = ((CheckBox)sender).IsChecked.Value;
                }
            };

            Controls?.Add(JsonFormatStackPanel);
            Controls?.Add(MinifyJsonCheckBox);
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

                    DataTable data_table = JsonConvert.DeserializeObject<DataTable?>(await reader.ReadToEndAsync()).Copy();

                    for (int i = 0; i < data_table.Columns.Count; ++i)
                    {
                        column_values.Add(data_table.Columns[i].ColumnName);
                    }

                    for (int i = 0; i < data_table.Rows.Count; ++i)
                    {
                        row_values.Add(data_table.Rows[i].ItemArray.Select(o => o.ToString()).ToArray());
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
                    case "Array of Objects":
                        {
                            Dictionary<string, object>[] json_objects = new Dictionary<string, object>[row_values.Length];

                            for (int i = 0; i < row_values.Length; ++i)
                            {
                                json_objects[i] = new Dictionary<string, object>();

                                for (int j = 0; j < column_values.Length; ++j)
                                {
                                    json_objects[i].Add(column_values[j].Replace(' ', '_'), row_values[i][j]);
                                }

                                Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, row_values.Length - 1, 0, 1000));
                            }

                            output = JsonConvert.SerializeObject(json_objects, MinifyJson ? Formatting.None : Formatting.Indented);

                            break;
                        }
                    case "2D Array":
                        {
                            string[][] json_array = new string[row_values.Length + 1][];

                            json_array[0] = column_values.Select(c => c.Replace(' ', '_')).ToArray();

                            for (int i = 0; i < row_values.Length; ++i)
                            {
                                json_array[i + 1] = row_values[i];

                                Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, row_values.Length - 1, 0, 1000));
                            }

                            output = JsonConvert.SerializeObject(json_array, MinifyJson ? Formatting.None : Formatting.Indented);

                            break;
                        }
                    case "Column Arrays":
                        {
                            Dictionary<string, string[]>[] json_objects = new Dictionary<string, string[]>[column_values.Length];

                            for (int i = 0; i < column_values.Length; ++i)
                            {
                                json_objects[i] = new Dictionary<string, string[]>()
                                {
                                    { column_values[i].Replace(' ', '_'), row_values.Select(row => row[i]).ToArray() }
                                };

                                Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, column_values.Length - 1, 0, 1000));
                            }

                            output = JsonConvert.SerializeObject(json_objects, MinifyJson ? Formatting.None : Formatting.Indented);

                            break;
                        }
                    case "Keyed Arrays":
                        {
                            Dictionary<int, string[]>[] json_objects = new Dictionary<int, string[]>[row_values.Length + 1];

                            json_objects[0] = new Dictionary<int, string[]>
                            {
                                { 0, column_values.Select(c => c.Replace(' ', '_')).ToArray() }
                            };

                            for (int i = 0; i < row_values.Length; ++i)
                            {
                                json_objects[i + 1] = new Dictionary<int, string[]>
                                {
                                    { i + 1, row_values[i] }
                                };

                                Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, row_values.Length - 1, 0, 1000));
                            }

                            output = JsonConvert.SerializeObject(json_objects, MinifyJson ? Formatting.None : Formatting.Indented);

                            break;
                        }
                }

                return output;
            });
        }
    }
}
