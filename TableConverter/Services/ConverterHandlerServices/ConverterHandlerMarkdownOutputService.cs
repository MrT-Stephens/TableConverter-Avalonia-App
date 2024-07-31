using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableConverter.DataModels;

namespace TableConverter.Services.ConverterHandlerServices
{
    internal class ConverterHandlerMarkdownOutputService : ConverterHandlerOutputAbstract
    {
        private readonly string[] TableTypes = [
            "Markdown Table (Normal)",
            "Markdown Table (Simple)",
        ];

        private string SelectedTableType = "Markdown Table (Normal)";

        private readonly Dictionary<string, HorizontalAlignment> TextAlignment = new()
        {
            { "Left", HorizontalAlignment.Left },
            { "Center", HorizontalAlignment.Center },
            { "Right", HorizontalAlignment.Right }
        };

        private string SelectedTextAlignment { get; set; } = "Left";

        private bool BoldColumnNames { get; set; } = false;

        private bool BoldFirstColumn { get; set; } = false;

        public override void InitializeControls()
        {
            Controls = new();

            var table_type_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var table_type_label = new Label()
            {
                Content = "Table Type:",
            };

            var table_type_combo_box = new ComboBox()
            {
                ItemsSource = TableTypes,
                SelectedItem = SelectedTableType,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            table_type_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_table_type)
                {
                    SelectedTableType = selected_table_type;
                }
            };

            table_type_stack_panel.Children.Add(table_type_label);
            table_type_stack_panel.Children.Add(table_type_combo_box);

            Controls?.Add(table_type_stack_panel);

            var text_alignment_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var text_alignment_label = new Label()
            {
                Content = "Text Alignment:",
            };

            var text_alignment_combo_box = new ComboBox()
            {
                ItemsSource = TextAlignment.Keys.ToArray(),
                SelectedItem = SelectedTextAlignment,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            text_alignment_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_text_alignment)
                {
                    SelectedTextAlignment = selected_text_alignment;
                }
            };

            text_alignment_stack_panel.Children.Add(text_alignment_label);
            text_alignment_stack_panel.Children.Add(text_alignment_combo_box);

            Controls?.Add(text_alignment_stack_panel);

            var bold_column_names_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            var bold_column_names_check_box = new CheckBox()
            {
                IsChecked = BoldColumnNames
            };

            bold_column_names_check_box.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox check_box)
                {
                    BoldColumnNames = check_box.IsChecked ?? false;
                }
            };

            var bold_column_names_label = new Label()
            {
                Content = "Bold Column Names",
                VerticalAlignment = VerticalAlignment.Center
            };

            bold_column_names_stack_panel.Children.Add(bold_column_names_check_box);
            bold_column_names_stack_panel.Children.Add(bold_column_names_label);

            Controls?.Add(bold_column_names_stack_panel);

            var bold_first_column_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            var bold_first_column_check_box = new CheckBox()
            {
                IsChecked = BoldFirstColumn
            };

            bold_first_column_check_box.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox check_box)
                {
                    BoldFirstColumn = check_box.IsChecked ?? false;
                }
            };

            var bold_first_column_label = new Label()
            {
                Content = "Bold First Column",
                VerticalAlignment = VerticalAlignment.Center
            };

            bold_first_column_stack_panel.Children.Add(bold_first_column_check_box);
            bold_first_column_stack_panel.Children.Add(bold_first_column_label);

            Controls?.Add(bold_first_column_stack_panel);
        }

        public override Task<string> ConvertAsync(string[] headers, string[][] rows)
        {
            return Task.Run(() =>
            {
                var ascii_output = new StringBuilder();

                // Calculates the max text character widths of every column.
                long[] max_column_widths = new long[headers.LongLength];

                for (long i = 0; i < headers.LongLength; i++)
                {
                    if ((i == 0 && BoldFirstColumn) || BoldColumnNames)
                    {
                        max_column_widths[i] = headers[i].LongCount() + 6;
                    }
                    else
                    {
                        max_column_widths[i] = headers[i].LongCount() + 2;
                    }
                }

                foreach (string[] row in rows)
                {
                    for (long i = 0; i < headers.LongLength; i++)
                    {
                        if (i == 0 && BoldFirstColumn)
                        {
                            max_column_widths[i] = Math.Max(max_column_widths[i], row[i].ToString().LongCount() + 6);
                        }
                        else
                        {
                            max_column_widths[i] = Math.Max(max_column_widths[i], row[i].ToString().LongCount() + 2);
                        }
                    }
                }

                string line = string.Empty;

                // Bold the data if the user has selected to bold the first column.
                if (BoldFirstColumn || BoldColumnNames)
                {
                    headers[0] = "**" + headers[0] + "**";
                }

                if (BoldColumnNames)
                {
                    for (long i = 1; i < headers.LongLength; i++)
                    {
                        headers[i] = "**" + headers[i] + "**";
                    }
                }

                if (BoldFirstColumn)
                {
                    foreach (var row in rows)
                    {
                        row[0] = "**" + row[0] + "**";
                    }
                }

                // Draw the table.
                switch (SelectedTableType)
                {
                    case "Markdown Table (Normal)":
                        {
                            ascii_output.AppendLine("|" + DrawDataRow(headers, max_column_widths, TextAlignment[SelectedTextAlignment], '|') + "|");
                            ascii_output.AppendLine("|" + DrawSeparator(max_column_widths, '|', '-') + "|");

                            foreach (var row in rows)
                            {
                                ascii_output.AppendLine("|" + DrawDataRow(row, max_column_widths, TextAlignment[SelectedTextAlignment], '|') + "|");
                            }

                            break;
                        }
                    case "Markdown Table (Simple)":
                        {
                            ascii_output.AppendLine(DrawDataRow(headers, max_column_widths, TextAlignment[SelectedTextAlignment], '|'));
                            ascii_output.AppendLine(DrawSeparator(max_column_widths, '|', '-'));

                            foreach (var row in rows)
                            {
                                ascii_output.AppendLine(DrawDataRow(row, max_column_widths, TextAlignment[SelectedTextAlignment], '|'));
                            }

                            break;
                        }
                }

                return ascii_output.ToString();
            });
        }

        private static string DrawSeparator(long[] column_widths, char intersection_char, char fill_char)
        {
            var separator = new StringBuilder();

            for (long i = 0; i < column_widths.LongLength; i++)
            {
                separator.Append(new string(fill_char, (int)column_widths[i]));

                separator.Append(i == column_widths.LongLength - 1 ? "" : intersection_char);
            }

            return separator.ToString();
        }

        private static string DrawDataRow(string[] row, long[] column_widths, HorizontalAlignment text_alignment, char intersection_char)
        {
            var data_row = new StringBuilder();

            for (long i = 0; i < row.LongLength; i++)
            {
                data_row.Append(MiscConverterHandlerItems.AlignText(row[i], text_alignment, (int)column_widths[i], ' '));

                data_row.Append(i == row.LongLength - 1 ? "" : intersection_char);
            }

            return data_row.ToString();
        }
    }
}
