using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlerServices
{
    internal class ConverterHandlerJsonLinesOutputService : ConverterHandlerOutputAbstract
    {
        private readonly string[] JsonLinesFormatTypes = [
            "Objects",
            "Arrays",
        ];

        private string SelectedJsonLinesFormatType = "Objects";

        public override void InitializeControls()
        {
            var json_lines_format_type_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var json_lines_format_type_label = new Label()
            {
                Content = "JSONLines File Format:",
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            var json_lines_format_type_combo_box = new ComboBox()
            {
                ItemsSource = JsonLinesFormatTypes,
                SelectedItem = SelectedJsonLinesFormatType,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            json_lines_format_type_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_json_lines_format_type)
                {
                    SelectedJsonLinesFormatType = selected_json_lines_format_type;
                }
            };

            json_lines_format_type_stack_panel.Children.Add(json_lines_format_type_label);
            json_lines_format_type_stack_panel.Children.Add(json_lines_format_type_combo_box);

            Controls?.Add(json_lines_format_type_stack_panel);
        }

        public override Task<string> ConvertAsync(string[] headers, string[][] rows, object? progress_bar)
        {
            return Task.Run(() =>
            {
                using (var writer = new StringWriter())
                {
                    switch (SelectedJsonLinesFormatType)
                    {
                        case "Objects":
                            {
                                for (long i = 0; i < rows.LongLength; i++)
                                {
                                    writer.Write("{");

                                    for (long j = 0; j < headers.LongLength; j++)
                                    {
                                        writer.Write($"\"{headers[j]}\":\"{rows[i][j]}\"");

                                        if (j != rows.LongLength - 1)
                                        {
                                            writer.Write(",");
                                        }
                                    }

                                    writer.Write("}");

                                    if (i != rows.LongLength - 1)
                                    {
                                        writer.Write(Environment.NewLine);
                                    }

                                    SetProgressBarValue(progress_bar, i, 0, rows.LongLength - 1);
                                }

                                break;
                            }
                        case "Arrays":
                            {
                                // Write headers
                                writer.Write("[");

                                writer.Write(string.Join(",", headers.Select(column => $"\"{column}\"").ToArray()));

                                writer.Write("]");

                                writer.Write(Environment.NewLine);

                                // Write rows
                                for (long i = 0; i < rows.LongLength; i++)
                                {
                                    writer.Write("[");

                                    writer.Write(string.Join(",", rows[i].Select(str => $"\"{str}\"").ToArray()));

                                    writer.Write("]");

                                    if (i != rows.LongLength - 1)
                                    {
                                        writer.Write(Environment.NewLine);
                                    }

                                    SetProgressBarValue(progress_bar, i, 0, rows.LongLength - 1);
                                }

                                break;
                            }
                    }

                    return writer.ToString();
                }
            });
        }
    }
}
