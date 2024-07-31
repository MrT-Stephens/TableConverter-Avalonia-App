using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System.Threading.Tasks;
using System;
using TableConverter.DataModels;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableConverter.Services.ConverterHandlerServices
{
    internal class ConverterHandlerSQLOutputService : ConverterHandlerOutputAbstract
    {
        private string TableName = "table_name";

        private readonly Dictionary<string, string> QuoteTypes = new()
        {
            { "No Quotes", "" },
            { "Double Quotes (\")", "\""},
            { "MySQL Quotes (`)", "`" },
            { "SQL Server Quotes ([])", "[" },
        };

        private string SelectedQuoteType = "No Quotes";

        private bool InsertMultiRowsAtOnce = false;

        public override void InitializeControls()
        {
            Controls = new();

            var table_name_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var table_name_label = new Label()
            {
                Content = "Table Name:",
            };

            var table_name_text_box = new TextBox()
            {
                Text = TableName,
            };

            table_name_text_box.TextChanged += (sender, e) =>
            {
                if (sender is TextBox text_box && !string.IsNullOrEmpty(text_box.Text))
                {
                    TableName = text_box.Text;
                }
            };

            table_name_stack_panel.Children.Add(table_name_label);
            table_name_stack_panel.Children.Add(table_name_text_box);

            Controls?.Add(table_name_stack_panel);

            var quote_type_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var quote_type_label = new Label()
            {
                Content = "Quote Type:",
            };

            var quote_type_combo_box = new ComboBox()
            {
                ItemsSource = QuoteTypes.Keys.ToArray(),
                SelectedItem = SelectedQuoteType,
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

            var insert_multi_rows_at_once_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            var insert_multi_rows_at_once_check_box = new CheckBox()
            {
                IsChecked = InsertMultiRowsAtOnce
            };

            insert_multi_rows_at_once_check_box.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox check_box)
                {
                    InsertMultiRowsAtOnce = check_box.IsChecked ?? false;
                }
            };

            var insert_multi_rows_at_once_label = new Label()
            {
                Content = "Insert Multiple Rows at Once",
                VerticalAlignment = VerticalAlignment.Center
            };

            insert_multi_rows_at_once_stack_panel.Children.Add(insert_multi_rows_at_once_check_box);
            insert_multi_rows_at_once_stack_panel.Children.Add(insert_multi_rows_at_once_label);

            Controls?.Add(insert_multi_rows_at_once_stack_panel);
        }

        public override Task<string> ConvertAsync(string[] headers, string[][] rows)
        {
            return Task.Run(() =>
            {
                var sql_builder = new StringBuilder();

                var headers_text = string.Join(", ", headers.Select(header =>
                        $"{QuoteTypes[SelectedQuoteType]}" +
                        $"{header.Replace(' ', '_')}" +
                        $"{(QuoteTypes[SelectedQuoteType] == "[" ? "]" : QuoteTypes[SelectedQuoteType])}"
                        ));

                for (long i = 0; i < rows.LongLength; i++)
                {
                    var row_text = string.Join(", ", rows[i].Select(val => $"\'{val.Replace("\'", "\'\'")}\'"));

                    if (InsertMultiRowsAtOnce)
                    {
                        if (i == 0)
                        {
                            sql_builder.Append($"INSERT INTO " +
                                    $"{QuoteTypes[SelectedQuoteType]}" +
                                    $"{TableName.Replace(' ', '_')}" +
                                    $"{(QuoteTypes[SelectedQuoteType] == "[" ? "]" : QuoteTypes[SelectedQuoteType])} " +
                                    $"({headers_text}) VALUES{Environment.NewLine} ({row_text})");
                        }
                        else
                        {
                            sql_builder.Append($",{Environment.NewLine} ({row_text})");

                            if (i == rows.LongLength - 1)
                            {
                                sql_builder.Append($";{Environment.NewLine}");
                            }
                        }
                    }
                    else
                    {
                        sql_builder.AppendLine($"INSERT INTO " +
                                $"{QuoteTypes[SelectedQuoteType]}" +
                                $"{TableName.Replace(' ', '_')}" +
                                $"{(QuoteTypes[SelectedQuoteType] == "[" ? "]" : QuoteTypes[SelectedQuoteType])} " +
                                $"({headers_text}) VALUES ({row_text});"
                                );
                    }
                }

                return sql_builder.ToString();
            });
        }
    }
}
