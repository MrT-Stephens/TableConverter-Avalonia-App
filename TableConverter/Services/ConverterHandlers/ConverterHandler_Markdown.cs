using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace TableConverter.Services.ConverterHandlers
{
    public class ConverterHandler_Markdown : ConverterHandlerBase
    {
        private string[] TableTypes { get; init; } =
        {
            "Markdown Table (Normal)",
            "Markdown Table (Simple)"
        };
        private string[] TextAlignments { get; init; } = { "Left", "Center", "Right" };

        private string CurrentTableType { get; set; } = "Markdown Table (Normal)";
        private string CurrentTextAlignment { get; set; } = "Left";
        private bool CurrentBoldHeader { get; set; } = false;
        private bool CurrentBoldColumn { get; set; } = false;

        public override void InitializeControls()
        {
            base.InitializeControls();

            var TableTypeStackPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };

            var TableTypeLabel = new Label { Content = "Table Type:" };

            var TableTypeComboBox = new ComboBox
            {
                ItemsSource = TableTypes,
                SelectedIndex = 0
            };

            TableTypeComboBox.SelectionChanged += (sender, e) =>
            {
                if (e.AddedItems.Count > 0)
                {
                    CurrentTableType = e.AddedItems[0].ToString();
                }
            };

            TableTypeStackPanel.Children.Add(TableTypeLabel);
            TableTypeStackPanel.Children.Add(TableTypeComboBox);

            var TextAlignmentStackPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };

            var TextAlignmentLabel = new Label { Content = "Text Alignment:" };

            var TextAlignmentComboBox = new ComboBox
            {
                ItemsSource = TextAlignments,
                SelectedIndex = 0
            };

            TextAlignmentComboBox.SelectionChanged += (sender, e) =>
            {
                if (e.AddedItems.Count > 0)
                {
                    CurrentTextAlignment = e.AddedItems[0].ToString();
                }
            };

            TextAlignmentStackPanel.Children.Add(TextAlignmentLabel);
            TextAlignmentStackPanel.Children.Add(TextAlignmentComboBox);

            var BoldHeaderCheckBox = new CheckBox
            {
                Content = "Bold Header",
                IsChecked = CurrentBoldHeader
            };

            BoldHeaderCheckBox.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox)
                {
                    CurrentBoldHeader = ((CheckBox)sender).IsChecked.Value;
                }
            };

            var BoldColumnCheckBox = new CheckBox
            {
                Content = "Bold Column",
                IsChecked = CurrentBoldColumn
            };

            BoldColumnCheckBox.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox)
                {
                    CurrentBoldColumn = ((CheckBox)sender).IsChecked.Value;
                }
            };

            Controls?.Add(TableTypeStackPanel);
            Controls?.Add(TextAlignmentStackPanel);
            Controls?.Add(BoldHeaderCheckBox);
            Controls?.Add(BoldColumnCheckBox);
        }

        public override Task<(List<string>, List<string[]>)> ConvertAsync(IStorageFile input)
        {
            return Task.Run(async () =>
            {
                List<string> column_values = new List<string>();
                List<string[]> rows_values = new List<string[]>();

                try
                {
                    bool first_line = true;
                    using var reader = new StreamReader(await input.OpenReadAsync());

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (first_line)
                        {
                            first_line = false;

                            if (line.StartsWith('|') && line.EndsWith('|'))
                            {
                                line = line.Substring(1, line.Length - 2);
                            }

                            string[] column_names = line.Split('|');

                            foreach (string column_name in column_names)
                            {
                                string temp_column = column_name.Trim();

                                if (temp_column.StartsWith("**") && temp_column.EndsWith("**"))
                                {
                                    temp_column = temp_column.Substring(2, temp_column.Length - 4);
                                }

                                column_values.Add(temp_column);
                            }
                        }
                        else if ((line.StartsWith("|-") && line.EndsWith("-|")) || (line.StartsWith(" -") && line.EndsWith("- ")) || (line.StartsWith('-')) || (line.EndsWith('-')))
                        {
                            continue;
                        }
                        else
                        {
                            if (line.StartsWith('|') && line.EndsWith('|'))
                            {
                                line = line.Substring(1, line.Length - 2);
                            }

                            string[] row = line.Split('|');

                            for (int i = 0; i < row.Length; ++i)
                            {
                                string temp_value = row[i].Trim();

                                if (temp_value.StartsWith("**") && temp_value.EndsWith("**"))
                                {
                                    temp_value = temp_value.Substring(2, temp_value.Length - 4);
                                }

                                row[i] = temp_value;
                            }

                            rows_values.Add(row);
                        }
                    }
                }
                catch (Exception)
                {
                    return (new List<string>(), new List<string[]>());
                }

                return (column_values, rows_values);
            });
        }

        public override Task<string> ConvertAsync(string[] column_values, string[][] row_values,  ProgressBar progress_bar)
        {
            return Task.Run(() =>
            {
                int progress_bar_value = 0;
                string output = string.Empty;

                // Calculates the max text character widths of every column.
                int[] max_column_widths = new int[column_values.Length];

                for (int i = 0; i < column_values.Length; ++i)
                {
                    if ((i == 0 && CurrentBoldColumn) || CurrentBoldHeader)
                    {
                        max_column_widths[i] = column_values[i].Length + 6;
                    }
                    else 
                    {
                        max_column_widths[i] = column_values[i].Length + 2;
                    }
                }

                foreach (string[] row in row_values)
                {
                    for (int i = 0; i < column_values.Length; ++i)
                    {
                        if (i == 0 && CurrentBoldColumn)
                        {
                            max_column_widths[i] = Math.Max(max_column_widths[i], row[i].ToString().Length + 6);
                        }
                        else
                        {
                            max_column_widths[i] = Math.Max(max_column_widths[i], row[i].ToString().Length + 2);
                        }
                    }

                    Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(progress_bar_value++, 1, (row_values.Length * 2) - 2, 0, 1000));
                }

                // Get the current text alignment.
                TextAlignment text_alignment = TextAlignment.Left;

                switch (CurrentTextAlignment)
                {
                    case "Left":
                        text_alignment = TextAlignment.Left;
                        break;
                    case "Center":
                        text_alignment = TextAlignment.Center;
                        break;
                    case "Right":
                        text_alignment = TextAlignment.Right;
                        break;
                }

                // Generate the table and store it in the output variable.
                switch (CurrentTableType)
                {
                    case "Markdown Table (Normal)":
                        {
                            for (int i = 0; i < column_values.Length; ++i)
                            {
                                output += "| " + AlignText(
                                    (((i == 0 && CurrentBoldColumn) || CurrentBoldHeader) ?  $"**{column_values[i]}**" : column_values[i])
                                    , text_alignment, max_column_widths[i] + 2, ' ') + " ";
                            }

                            output += "|" + Environment.NewLine;

                            for (int i = 0; i < column_values.Length; ++i)
                            {
                                output += "|-" + AlignText("", text_alignment, max_column_widths[i] + 2, '-') + "-";
                            }

                            output += "|" + Environment.NewLine;

                            for (int i = 0; i < row_values.Length; ++i)
                            {
                                for (int j = 0; j < column_values.Length; ++j)
                                {
                                    output += "| " + AlignText(
                                        ((j == 0 && CurrentBoldColumn) ? $"**{row_values[i][j]}**" : row_values[i][j].ToString())
                                        , text_alignment, max_column_widths[j] + 2, ' ') + " ";
                                }

                                output += "|" + Environment.NewLine;

                                Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(progress_bar_value++, 1, (row_values.Length * 2) - 2, 0, 1000));
                            }

                            break;
                        }
                    case "Markdown Table (Simple)":
                        {
                            for (int i = 0; i < column_values.Length; ++i)
                            {
                                output += " " + AlignText(
                                    (((i == 0 && CurrentBoldColumn) || CurrentBoldHeader) ? $"**{column_values[i]}**" : column_values[i])
                                    , text_alignment, max_column_widths[i] + 2, ' ') + (i == column_values.Length - 1 ? $" {Environment.NewLine}" : " |");
                            }

                            for (int i = 0; i < column_values.Length; ++i)
                            {
                                output += " " + AlignText("", text_alignment, max_column_widths[i] + 2, '-') + (i == column_values.Length - 1 ? $" {Environment.NewLine}" : " |");
                            }

                            for (int i = 0; i < row_values.Length; ++i)
                            {
                                for (int j = 0; j < column_values.Length; ++j)
                                {
                                    output += " " + AlignText(
                                        ((j == 0 && CurrentBoldColumn) ? $"**{row_values[i][j]}**" : row_values[i][j].ToString())
                                        , text_alignment, max_column_widths[j] + 2, ' ') + (j == column_values.Length - 1 ? $" {Environment.NewLine}" : " |");
                                }

                                Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(progress_bar_value++, 0, row_values.Length * 2, 0, 1000));
                            }

                            break;
                        }
                }

                return output;
            });
        }
    }
}