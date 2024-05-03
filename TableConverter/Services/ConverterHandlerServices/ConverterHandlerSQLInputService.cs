using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TableConverter.DataModels;
using TableConverter.Interfaces;

namespace TableConverter.Services.ConverterHandlerServices
{
    internal class ConverterHandlerSQLInputService : ConverterHandlerInputAbstract
    {
        private readonly Dictionary<string, string> QuoteTypes = new()
        {
            { "No Quotes", "" },
            { "Double Quotes (\")", "\""},
            { "MySQL Quotes (`)", "`" },
            { "SQL Server Quotes ([])", "[" },
        };

        private string SelectedQuoteType = "No Quotes";

        private bool HasColumnNames = true;

        public override void InitializeControls()
        {
            var quote_type_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var quote_type_label = new Label()
            {
                Content = "Quote Type:",
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            var quote_type_combo_box = new ComboBox()
            {
                ItemsSource = QuoteTypes.Keys.ToArray(),
                SelectedItem = SelectedQuoteType,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            quote_type_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_quote_type)
                {
                    SelectedQuoteType = selected_quote_type;
                }
            };

            quote_type_stack_panel.Children.Add(quote_type_label);
            quote_type_stack_panel.Children.Add(quote_type_combo_box);

            Controls?.Add(quote_type_stack_panel);

            var has_column_names_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            var has_column_names_check_box = new CheckBox()
            {
                IsChecked = HasColumnNames
            };

            has_column_names_check_box.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox check_box)
                {
                    HasColumnNames = check_box.IsChecked ?? false;
                }
            };

            var has_column_names_label = new Label()
            {
                Content = "Has Column Names",
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
                VerticalAlignment = VerticalAlignment.Center
            };

            has_column_names_stack_panel.Children.Add(has_column_names_check_box);
            has_column_names_stack_panel.Children.Add(has_column_names_label);

            Controls?.Add(has_column_names_stack_panel);
        }

        public override Task<TableData> ReadTextAsync(string text)
        {
            return Task.Run(() =>
            {
                var headers = new List<string>();
                var rows = new List<string[]>();

                string sql_regex_w_column_names = @"INSERT\sINTO\s([`""\[]?\w+[`""\]]?)\s*\((.*?)\)\s*VALUES\s*\((.*?)\);";
                string sql_regex_wo_column_names = @"INSERT\sINTO\s([`""\[]?\w+[`""\]]?)\s*VALUES\s*\((.*?)\);";

                if (HasColumnNames)
                {
                    MatchCollection matches = Regex.Matches(text, sql_regex_w_column_names, RegexOptions.Singleline);

                    bool first_loop = true;

                    foreach (Match match in matches.Cast<Match>())
                    {
                        string[] columns = match.Groups[2].Value.Split(',');
                        string[] values = match.Groups[3].Value.Split(',');

                        if (first_loop)
                        {
                            first_loop = false;

                            for (long i = 0; i < columns.Length; i++)
                            {
                                if (columns[i].StartsWith(SelectedQuoteType) && columns[i].EndsWith(SelectedQuoteType == "[" ? "]" : SelectedQuoteType))
                                {
                                    columns[i] = columns[i].Substring(1, columns[i].Length - 2);
                                    headers.Add(columns[i].Trim());
                                }
                                else
                                {
                                    headers.Add(columns[i].Trim());
                                }
                            }
                        }

                        rows.Add(values.Select(value => value.Trim().Trim('\'')).ToArray());
                    }
                }
                else
                {
                    MatchCollection matches = Regex.Matches(text, sql_regex_wo_column_names, RegexOptions.Singleline);

                    foreach (Match match in matches.Cast<Match>())
                    {
                        string[] values = match.Groups[2].Value.Split(',');

                        rows.Add(values.Select(value => value.Trim().Trim('\'')).ToArray());
                    }

                    headers.AddRange(Enumerable.Range(1, rows[0].Length).Select(i => $"Column {i}"));
                }

                return new TableData(headers, rows);
            });
        }
    }
}
