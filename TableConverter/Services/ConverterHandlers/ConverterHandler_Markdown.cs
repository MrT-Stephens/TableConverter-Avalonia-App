using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;

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

                                data_table.Columns.Add(temp_column);
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

                            string[] row_values = line.Split('|');

                            DataRow row = data_table.NewRow();

                            for (int i = 0; i < row_values.Length; ++i)
                            {
                                string temp_value = row_values[i].Trim();

                                if (temp_value.StartsWith("**") && temp_value.EndsWith("**"))
                                {
                                    temp_value = temp_value.Substring(2, temp_value.Length - 4);
                                }

                                row[i] = temp_value;
                            }

                            data_table.Rows.Add(row);
                        }
                    }
                }
                catch (Exception)
                {
                    return new DataTable();
                }

                return data_table;
            });
        }

        public override Task<string> ConvertAsync(DataTable data_table, ProgressBar progress_bar)
        {
            return Task.Run(() =>
            {
                int progress_bar_value = 0;
                string output = string.Empty;

                // Calculates the max text character widths of every column.
                int[] max_column_widths = new int[data_table.Columns.Count];

                for (int i = 0; i < data_table.Columns.Count; ++i)
                {
                    if ((i == 0 && CurrentBoldColumn) || CurrentBoldHeader)
                    {
                        max_column_widths[i] = data_table.Columns[i].ColumnName.Length + 6;
                    }
                    else 
                    {
                        max_column_widths[i] = data_table.Columns[i].ColumnName.Length + 2;
                    }
                }

                foreach (DataRow row in data_table.Rows)
                {
                    for (int i = 0; i < data_table.Columns.Count; ++i)
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

                    Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(progress_bar_value++, 1, (data_table.Rows.Count * 2) - 2, 0, 1000));
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
                            for (int i = 0; i < data_table.Columns.Count; ++i)
                            {
                                output += "| " + AlignText(
                                    (((i == 0 && CurrentBoldColumn) || CurrentBoldHeader) ?  $"**{data_table.Columns[i].ColumnName}**" : data_table.Columns[i].ColumnName)
                                    , text_alignment, max_column_widths[i] + 2, ' ') + " ";
                            }

                            output += "|" + Environment.NewLine;

                            for (int i = 0; i < data_table.Columns.Count; ++i)
                            {
                                output += "|-" + AlignText("", text_alignment, max_column_widths[i] + 2, '-') + "-";
                            }

                            output += "|" + Environment.NewLine;

                            for (int i = 0; i < data_table.Rows.Count; ++i)
                            {
                                for (int j = 0; j < data_table.Columns.Count; ++j)
                                {
                                    output += "| " + AlignText(
                                        ((j == 0 && CurrentBoldColumn) ? $"**{data_table.Rows[i][j]}**" : data_table.Rows[i][j].ToString())
                                        , text_alignment, max_column_widths[j] + 2, ' ') + " ";
                                }

                                output += "|" + Environment.NewLine;

                                Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(progress_bar_value++, 1, (data_table.Rows.Count * 2) - 2, 0, 1000));
                            }

                            break;
                        }
                    case "Markdown Table (Simple)":
                        {
                            for (int i = 0; i < data_table.Columns.Count; ++i)
                            {
                                output += " " + AlignText(
                                    (((i == 0 && CurrentBoldColumn) || CurrentBoldHeader) ? $"**{data_table.Columns[i].ColumnName}**" : data_table.Columns[i].ColumnName)
                                    , text_alignment, max_column_widths[i] + 2, ' ') + (i == data_table.Columns.Count - 1 ? $" {Environment.NewLine}" : " |");
                            }

                            for (int i = 0; i < data_table.Columns.Count; ++i)
                            {
                                output += " " + AlignText("", text_alignment, max_column_widths[i] + 2, '-') + (i == data_table.Columns.Count - 1 ? $" {Environment.NewLine}" : " |");
                            }

                            for (int i = 0; i < data_table.Rows.Count; ++i)
                            {
                                for (int j = 0; j < data_table.Columns.Count; ++j)
                                {
                                    output += " " + AlignText(
                                        ((j == 0 && CurrentBoldColumn) ? $"**{data_table.Rows[i][j]}**" : data_table.Rows[i][j].ToString())
                                        , text_alignment, max_column_widths[j] + 2, ' ') + (j == data_table.Columns.Count - 1 ? $" {Environment.NewLine}" : " |");
                                }

                                Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(progress_bar_value++, 0, data_table.Rows.Count * 2, 0, 1000));
                            }

                            break;
                        }
                }

                return output;
            });
        }
    }
}