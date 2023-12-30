using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace TableConverter.Services.ConverterHandlers
{
    public class ConverterHandler_Ascii : ConverterHandlerBase
    {
        string[] TableTypes { get; init; } = {
            "Ascii Table (MySQL)",
            "Ascii Table (Dots)",
            "Ascii Table (Compact)",
            "Ascii Table (Simple)",
            "Ascii Table (Wavy)",
            "UTF-8 Table (Single Line)",
            "UTF-8 Table (Double Line)"
        };
        string[] TextAlignments { get; init; } = { "Left", "Center", "Right" };
        string[] CommentTypes { get; init; } = {
            "No Comments",
            "//",
            "--",
            "#",
            "%",
            ";",
            "*"
        };

        private string CurrentTableType { get; set; } = "Ascii Table (MySQL)";
        private string CurrentTextAlignment { get; set; } = "Left";
        private string CurrentCommentType { get; set; } = "No Comments";
        private bool CurrentForceSeparation { get; set; } = false;

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

            var CommentTypeStackPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };

            var CommentTypeLabel = new Label { Content = "Comment Type:" };

            var CommentTypeComboBox = new ComboBox
            {
                ItemsSource = CommentTypes,
                SelectedIndex = 0
            };

            CommentTypeComboBox.SelectionChanged += (sender, e) =>
            {
                if (e.AddedItems.Count > 0)
                {
                    CurrentCommentType = e.AddedItems[0].ToString();
                }
            };

            CommentTypeStackPanel.Children.Add(CommentTypeLabel);
            CommentTypeStackPanel.Children.Add(CommentTypeComboBox);

            var ForceSeparationCheckBox = new CheckBox
            {
                Content = "Force Separation ",
                IsChecked = CurrentForceSeparation
            };

            ForceSeparationCheckBox.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox)
                {
                    CurrentForceSeparation = ((CheckBox)sender).IsChecked.Value;
                }
            };

            Controls?.Add(TableTypeStackPanel);
            Controls?.Add(TextAlignmentStackPanel);
            Controls?.Add(CommentTypeStackPanel);
            Controls?.Add(ForceSeparationCheckBox);
        }

        public override Task<string> ConvertAsync(DataTable input, ProgressBar progress_bar)
        {
            return Task.Run(() =>
            {
                string output = string.Empty;
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

                switch (CurrentTableType)
                {
                    case "Ascii Table (MySQL)":
                        output = PrintTable(input, progress_bar, '-', '|', '+', text_alignment, CurrentForceSeparation, CurrentCommentType == "No Comments" ? "" : CurrentCommentType + "    ");
                        break;
                    case "Ascii Table (Dots)":
                        output = PrintTable(input, progress_bar, '.', ':', '.', text_alignment, CurrentForceSeparation, CurrentCommentType == "No Comments" ? "" : CurrentCommentType + "    ");
                        break;
                    case "Ascii Table (Compact)":
                        output = PrintTable(input, progress_bar, '-', ' ', '-', text_alignment, CurrentForceSeparation, CurrentCommentType == "No Comments" ? "" : CurrentCommentType + "    ");
                        break;
                    case "Ascii Table (Simple)":
                        output = PrintTable(input, progress_bar, '=', ' ', '=', text_alignment, CurrentForceSeparation, CurrentCommentType == "No Comments" ? "" : CurrentCommentType + "    ");
                        break;
                    case "Ascii Table (Wavy)":
                        output = PrintTable(input, progress_bar, '~', '|', '+', text_alignment, CurrentForceSeparation, CurrentCommentType == "No Comments" ? "" : CurrentCommentType + "    ");
                        break;
                    case "UTF-8 Table (Single Line)":
                        output = PrintTable(input, progress_bar, '─', '│', '┼', text_alignment, CurrentForceSeparation, CurrentCommentType == "No Comments" ? "" : CurrentCommentType + "    ");
                        break;
                    case "UTF-8 Table (Double Line)":
                        output = PrintTable(input, progress_bar, '═', '║', '╬', text_alignment, CurrentForceSeparation, CurrentCommentType == "No Comments" ? "" : CurrentCommentType + "    ");
                        break;
                }

                return output;
            });
        }

        public static string PrintTable(DataTable data_table, ProgressBar progress_bar, char horizontal_char, char vertical_char, char intersection_char, TextAlignment text_alignment, bool force_row_separation, string comment_char)
        {
            int progress_bar_value = 0;
            string output = string.Empty;

            // Calculates the max text character widths of every column.
            int[] max_column_widths = new int[data_table.Columns.Count];

            for (int i = 0; i < data_table.Columns.Count; ++i)
            {
                max_column_widths[i] = data_table.Columns[i].ColumnName.Length + 2;
            }

            foreach (DataRow row in data_table.Rows)
            {
                for (int i = 0; i < data_table.Columns.Count; ++i)
                {
                    max_column_widths[i] = Math.Max(max_column_widths[i], row[i].ToString().Length + 2);

                    Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(progress_bar_value++, 0, data_table.Rows.Count * 2, 0, 1000));
                }
            }

            // Prints the header of the table.
            output += comment_char;
            output += PrintSeparator(max_column_widths, horizontal_char, intersection_char);
            output += comment_char;
            output += PrintRow(data_table.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray(), max_column_widths, vertical_char, text_alignment);

            // Prints the rows of the table.
            for (int i = 0; i < data_table.Rows.Count; ++i)
            {
                if (force_row_separation || i == 0)
                {
                    output += comment_char;
                    output += PrintSeparator(max_column_widths, horizontal_char, intersection_char);
                }

                output += comment_char;
                output += PrintRow(data_table.Rows[i].ItemArray.Select(item => item.ToString()).ToArray(), max_column_widths, vertical_char, text_alignment);

                if (i == data_table.Rows.Count - 1)
                {
                    output += comment_char;
                    output += PrintSeparator(max_column_widths, horizontal_char, intersection_char);
                }

                Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(progress_bar_value++, 0, data_table.Rows.Count * 2, 0, 1000));
            }

            return output;
        }

        public static string PrintSeparator(int[] max_column_widths, char horizontal_char, char intersection_char)
        {
            string output = string.Empty;

            for (int i = 0; i < max_column_widths.Length; ++i)
            {
                output += intersection_char;

                output += new string(horizontal_char, max_column_widths[i] + 2);

                if (i == max_column_widths.Length - 1)
                {
                    output += intersection_char;
                }
            }

            output += Environment.NewLine;

            return output;
        }

        public static string PrintRow(string[] row, int[] max_column_widths, char vertical_char, TextAlignment text_alignment)
        {
            string output = string.Empty;

            for (int i = 0; i < row.Length; ++i)
            {
                output += $"{(i == 0 ? "" : " ")}{vertical_char} ";

                output += AlignText(row[i], text_alignment, max_column_widths[i], ' ');

                if (i == row.Length - 1)
                {
                    output += $" {vertical_char}";
                }
            }

            output += Environment.NewLine;

            return output;
        }
    }
}
