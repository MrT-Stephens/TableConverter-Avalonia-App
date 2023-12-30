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

        public override Task<DataTable> ConvertAsync(IStorageFile input)
        {
            return Task.Run(async () =>
            {
                DataTable data_table = new DataTable();

                try
                {
                    using var reader = new StreamReader(await input.OpenReadAsync());

                    data_table = JsonConvert.DeserializeObject<DataTable?>(await reader.ReadToEndAsync()).Copy();
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
                    case "Array of Objects":
                        {
                            Dictionary<string, object>[] json_objects = new Dictionary<string, object>[input.Rows.Count];

                            for (int i = 0; i < input.Rows.Count; ++i)
                            {
                                json_objects[i] = input.Rows[i].Table.Columns.Cast<DataColumn>().ToDictionary(c => c.ColumnName.Replace(' ', '_'), c => input.Rows[i][c]);

                                Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, input.Rows.Count - 1, 0, 1000));
                            }

                            output = JsonConvert.SerializeObject(json_objects, MinifyJson ? Formatting.None : Formatting.Indented);

                            break;
                        }
                    case "2D Array":
                        {
                            string[][] json_array = new string[input.Rows.Count + 1][];

                            json_array[0] = input.Columns.Cast<DataColumn>().Select(c => c.ColumnName.Replace(' ', '_')).ToArray();

                            for (int i = 0; i < input.Rows.Count; ++i)
                            {
                                json_array[i + 1] = input.Rows[i].ItemArray.Select(o => o.ToString()).ToArray();

                                Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, input.Rows.Count - 1, 0, 1000));
                            }

                            output = JsonConvert.SerializeObject(json_array, MinifyJson ? Formatting.None : Formatting.Indented);

                            break;
                        }
                    case "Column Arrays":
                        {
                            Dictionary<string, string[]>[] json_obejcts = new Dictionary<string, string[]>[input.Columns.Count];

                            for (int i = 0; i < input.Columns.Count; ++i)
                            {
                                json_obejcts[i] = new Dictionary<string, string[]>()
                                {
                                    { input.Columns[i].ColumnName.Replace(' ', '_'), input.Rows.Cast<DataRow>().Select(r => r[i].ToString()).ToArray() }
                                };

                                Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, input.Columns.Count - 1, 0, 1000));
                            }

                            output = JsonConvert.SerializeObject(json_obejcts, MinifyJson ? Formatting.None : Formatting.Indented);

                            break;
                        }
                    case "Keyed Arrays":
                        {
                            Dictionary<int, string[]>[] json_obejcts = new Dictionary<int, string[]>[input.Rows.Count + 1];

                            json_obejcts[0] = new Dictionary<int, string[]> 
                            {
                                { 0, input.Columns.Cast<DataColumn>().Select(c => c.ColumnName.Replace(' ', '_')).ToArray() }
                            };

                            for (int i = 0; i < input.Rows.Count; ++i)
                            {
                                json_obejcts[i + 1] = new Dictionary<int, string[]>
                                {
                                    { i + 1, input.Rows[i].ItemArray.Select(o => o.ToString()).ToArray() }
                                };

                                Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, input.Rows.Count - 1, 0, 1000));
                            }

                            output = JsonConvert.SerializeObject(json_obejcts, MinifyJson ? Formatting.None : Formatting.Indented);

                            break;
                        }
                }

                return output;
            });
        }
    }
}
