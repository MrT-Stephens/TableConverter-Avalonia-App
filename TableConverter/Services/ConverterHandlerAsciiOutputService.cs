using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using CsvHelper.Configuration;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableConverter.Interfaces;
using Avalonia.Threading;

namespace TableConverter.Services
{
    internal class ConverterHandlerAsciiOutputService : ConverterHandlerOutputAbstract
    {
        private record TableCharacterConfig(
            char HeaderTopLeft,
            char HeaderTopRight,
            char HeaderBottomLeft,
            char HeaderBottomRight,
            char BottomLeft,
            char BottomRight,
            char Horizontal,
            char Vertical,
            char MiddleIntersection,
            char TopIntersection,
            char BottomIntersection,
            char LeftIntersection,
            char RightIntersection
        );

        private readonly Dictionary<string, TableCharacterConfig> TableTypes = new()
        {
            {
                "Single",
                new TableCharacterConfig(
                                       '┌', '┐', '└', '┘', '└', '┘', '─', '│', '┼', '┬', '┴', '├', '┤'
                                   )
            },
            {
                "Double",
                new TableCharacterConfig(
                                       '╔', '╗', '╚', '╝', '╚', '╝', '═', '║', '╬', '╦', '╩', '╠', '╣'
                                   )
            },
            {
                "Bold",
                new TableCharacterConfig(
                                       '┏', '┓', '┗', '┛', '┗', '┛', '━', '┃', '╋', '┳', '┻', '┣', '┫'
                                   )
            },
            {
                "Round",
                new TableCharacterConfig(
                                       '╭', '╮', '╰', '╯', '╰', '╯', '─', '│', '┼', '┬', '┴', '├', '┤'
                                   )
            },
            {
                "Bold Round",
                new TableCharacterConfig(
                                       '╭', '╮', '╰', '╯', '╰', '╯', '━', '┃', '╋', '┳', '┻', '┣', '┫'
                                   )
            },
            {
                "Classic",
                new TableCharacterConfig(
                                       '+', '+', '+', '+', '+', '+', '-', '|', '+', '+', '+', '+', '+'
                                   )
            },
        };

        private string SelectedTableType { get; set; } = "Single";

        private readonly Dictionary<string, HorizontalAlignment> TextAlignment = new()
        {
            { "Left", HorizontalAlignment.Left },
            { "Center", HorizontalAlignment.Center },
            { "Right", HorizontalAlignment.Right }
        };

        private string SelectedTextAlignment { get; set; } = "Left";

        private readonly Dictionary<string, string> CommentTypes = new()
        {
            { "None", "" },
            { "Double-Slash (//)", "//" },
            { "Hash (#)", "#" },
            { "Semi-Colon (;)", ";" },
            { "Double-Dash (--)","--" },
            { "Percent (%)", "%" },
            { "Asterisk (*)", "*" }
        };

        private string SelectedCommentType { get; set; } = "None";

        private bool ForceRowSeparators = false;

        public override void InitializeControls()
        {
            var table_type_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var table_type_label = new Label()
            {
                Content = "Table Type:",
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            var table_type_combo_box = new ComboBox()
            {
                ItemsSource = TableTypes.Keys.ToArray(),
                SelectedItem = SelectedTableType,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
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
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            var text_alignment_combo_box = new ComboBox()
            {
                ItemsSource = TextAlignment.Keys.ToArray(),
                SelectedItem = SelectedTextAlignment,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
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

            var comment_type_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var comment_type_label = new Label()
            {
                Content = "Comment Type:",
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            var comment_type_combo_box = new ComboBox()
            {
                ItemsSource = CommentTypes.Keys.ToArray(),
                SelectedItem = SelectedCommentType,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            comment_type_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_comment_type)
                {
                    SelectedCommentType = selected_comment_type;
                }
            };

            comment_type_stack_panel.Children.Add(comment_type_label);
            comment_type_stack_panel.Children.Add(comment_type_combo_box);

            Controls?.Add(comment_type_stack_panel);

            var force_row_separators_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };

            var force_row_separators_check_box = new CheckBox()
            {
                IsChecked = ForceRowSeparators
            };

            force_row_separators_check_box.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox check_box)
                {
                    ForceRowSeparators = check_box.IsChecked ?? false;
                }
            };

            var force_row_separators_label = new Label()
            {
                Content = "Force Row Separators",
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
                VerticalAlignment = VerticalAlignment.Center
            };

            force_row_separators_stack_panel.Children.Add(force_row_separators_check_box);
            force_row_separators_stack_panel.Children.Add(force_row_separators_label);

            Controls?.Add(force_row_separators_stack_panel);
        }

        public override Task<string> ConvertAsync(string[] headers, string[][] rows, object? progress_bar)
        {
            return Task.Run(() =>
            {
                var ascii_output = new StringBuilder();

                var table_character_config = TableTypes[SelectedTableType];

                // Calculates the max text character widths of every column.
                long[] max_column_widths = new long[headers.LongLength];

                for (long i = 0; i < headers.LongLength; i++)
                {
                    max_column_widths[i] = headers[i].Length + 2;
                }

                foreach (string[] row in rows)
                {
                    for (long i = 0; i < row.LongLength; i++)
                    {
                        max_column_widths[i] = Math.Max(max_column_widths[i], row[i].ToString().LongCount() + 2);
                    }
                }

                string comment = SelectedCommentType == CommentTypes.First().Key ? "" : $"{CommentTypes[SelectedCommentType]}    ";

                // Draws the table header.
                ascii_output.AppendLine(comment +
                        DrawSeparator(max_column_widths, 
                        table_character_config.HeaderTopLeft, 
                        table_character_config.HeaderTopRight, 
                        table_character_config.TopIntersection,
                        table_character_config.Horizontal));

                ascii_output.AppendLine(comment +
                        DrawDataRow(headers, max_column_widths,
                        TextAlignment[SelectedTextAlignment],
                        table_character_config.Vertical,
                        table_character_config.Vertical,
                        table_character_config.Vertical));

                ascii_output.AppendLine(comment +
                        DrawSeparator(max_column_widths,
                        table_character_config.LeftIntersection,
                        table_character_config.RightIntersection,
                        table_character_config.MiddleIntersection,
                        table_character_config.Horizontal));

                // Draws the table rows.
                for (long i = 0; i < rows.LongLength; i++)
                {
                    ascii_output.AppendLine(comment +
                            DrawDataRow(rows[i], max_column_widths,
                            TextAlignment[SelectedTextAlignment],
                            table_character_config.Vertical,
                            table_character_config.Vertical,
                            table_character_config.Vertical));

                    if (i < rows.LongLength - 1 && ForceRowSeparators)
                    {
                        ascii_output.AppendLine(comment +
                                DrawSeparator(max_column_widths,
                                table_character_config.LeftIntersection,
                                table_character_config.RightIntersection,
                                table_character_config.MiddleIntersection,
                                table_character_config.Horizontal));
                    }
                    else if (i == rows.LongLength - 1)
                    {
                        ascii_output.AppendLine(comment +
                                DrawSeparator(max_column_widths,
                                table_character_config.BottomLeft,
                                table_character_config.BottomRight,
                                table_character_config.BottomIntersection,
                                table_character_config.Horizontal));
                    }

                    SetProgressBarValue(progress_bar, i, 0, rows.LongLength - 1);
                }

                return ascii_output.ToString();
            });
        }

        private static string DrawSeparator(long[] column_widths, char left_char, char right_char, char intersection_char, char fill_char)
        {
            var separator = new StringBuilder();

            separator.Append(left_char);

            for (long i = 0; i < column_widths.LongLength; i++)
            {
                separator.Append(new string(fill_char, (int)column_widths[i]));

                separator.Append(i == column_widths.LongLength - 1 ? right_char : intersection_char);
            }

            return separator.ToString();
        }

        private static string DrawDataRow(string[] row, long[] column_widths, HorizontalAlignment text_alignment, char left_char, char right_char, char intersection_char)
        {
            var data_row = new StringBuilder();

            data_row.Append(left_char);

            for (long i = 0; i < row.LongLength; i++)
            {
                data_row.Append(MiscConverterHandlerItems.AlignText(row[i], text_alignment, (int)column_widths[i], ' '));

                data_row.Append(i == row.LongLength - 1 ? right_char : intersection_char);
            }

            return data_row.ToString();
        }
    }
}
