using System.Threading.Tasks;
using Newtonsoft.Json;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using TableConverter.DataModels;

namespace TableConverter.Services.ConverterHandlerServices
{
    internal class ConverterHandlerJsonOutputService : ConverterHandlerOutputAbstract
    {
        private readonly string[] JsonFormatTypes = [
            "Array of Objects",
            "2D Arrays",
            "Column Arrays",
            "Keyed Arrays",
        ];

        private string SelectedJsonFormatType = "Array of Objects";

        private bool MinifyJson = false;

        public override void InitializeControls()
        {
            var json_format_type_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var json_format_type_label = new Label()
            {
                Content = "JSON File Format:",
            };

            var json_format_type_combo_box = new ComboBox()
            {
                ItemsSource = JsonFormatTypes,
                SelectedItem = SelectedJsonFormatType,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            json_format_type_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_json_format_type)
                {
                    SelectedJsonFormatType = selected_json_format_type;
                }
            };

            json_format_type_stack_panel.Children.Add(json_format_type_label);
            json_format_type_stack_panel.Children.Add(json_format_type_combo_box);

            Controls?.Add(json_format_type_stack_panel);

            var minify_json_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            var minify_json_check_box = new CheckBox()
            {
                IsChecked = MinifyJson
            };

            minify_json_check_box.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox check_box)
                {
                    MinifyJson = check_box.IsChecked ?? false;
                }
            };

            var minify_json_label = new Label()
            {
                Content = "Minify JSON",
                VerticalAlignment = VerticalAlignment.Center
            };

            minify_json_stack_panel.Children.Add(minify_json_check_box);
            minify_json_stack_panel.Children.Add(minify_json_label);

            Controls?.Add(minify_json_stack_panel);
        }

        public override Task<string> ConvertAsync(string[] headers, string[][] rows, object? progress_bar)
        {
            return Task.Run(() =>
            {
                switch (SelectedJsonFormatType)
                {
                    case "Array of Objects":
                        {
                            Dictionary<string, object>[] json_objects = new Dictionary<string, object>[rows.LongLength];

                            for (long i = 0; i < rows.LongLength; i++)
                            {
                                json_objects[i] = new Dictionary<string, object>();

                                for (long j = 0; j < headers.LongLength; j++)
                                {
                                    json_objects[i].Add(headers[j].Replace(' ', '_'), rows[i][j]);
                                }

                                SetProgressBarValue(progress_bar, i, 0, rows.LongLength - 1);
                            }

                            return JsonConvert.SerializeObject(json_objects, MinifyJson ? Formatting.None : Formatting.Indented);
                        }
                    case "2D Arrays":
                        {
                            string[][] json_array = new string[rows.LongLength + 1][];

                            json_array[0] = headers.Select(c => c.Replace(' ', '_')).ToArray();

                            for (long i = 0; i < rows.LongLength; i++)
                            {
                                json_array[i + 1] = rows[i];

                                SetProgressBarValue(progress_bar, i, 0, rows.LongLength - 1);
                            }

                            return JsonConvert.SerializeObject(json_array, MinifyJson ? Formatting.None : Formatting.Indented);
                        }
                    case "Column Arrays":
                        {
                            Dictionary<string, string[]>[] json_objects = new Dictionary<string, string[]>[headers.LongLength];

                            for (long i = 0; i < headers.LongLength; i++)
                            {
                                json_objects[i] = new Dictionary<string, string[]>()
                                {
                                    { headers[i].Replace(' ', '_'), rows.Select(row => row[i]).ToArray() }
                                };

                                SetProgressBarValue(progress_bar, i, 0, headers.LongLength - 1);
                            }

                            return JsonConvert.SerializeObject(json_objects, MinifyJson ? Formatting.None : Formatting.Indented);
                        }
                    case "Keyed Arrays":
                        {
                            Dictionary<long, string[]>[] json_objects = new Dictionary<long, string[]>[rows.LongLength + 1];

                            json_objects[0] = new Dictionary<long, string[]>
                            {
                                { 0, headers.Select(c => c.Replace(' ', '_')).ToArray() }
                            };

                            for (long i = 0; i < rows.LongLength; i++)
                            {
                                json_objects[i + 1] = new Dictionary<long, string[]>()
                                {
                                    { i + 1, rows[i] }
                                };

                                SetProgressBarValue(progress_bar, i, 0, rows.LongLength - 1);
                            }

                            return JsonConvert.SerializeObject(json_objects, MinifyJson ? Formatting.None : Formatting.Indented);
                        }
                }

                return string.Empty;
            });
        }
    }
}
