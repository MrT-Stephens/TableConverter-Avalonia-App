using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System.Threading.Tasks;
using System;
using TableConverter.Interfaces;
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

        public override void InitializeControls()
        {
            var table_name_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var table_name_label = new Label()
            {
                Content = "Table Name:",
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            var table_name_text_box = new TextBox()
            {
                Text = TableName,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
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
        }

        public override Task<string> ConvertAsync(string[] headers, string[][] rows, object? progress_bar)
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
                    var row_text = string.Join(", ", rows[i].Select(val => $"\'{val}\'"));

                    sql_builder.AppendLine($"INSERT INTO " +
                            $"{QuoteTypes[SelectedQuoteType]}" +
                            $"{TableName.Replace(' ', '_')}" +
                            $"{(QuoteTypes[SelectedQuoteType] == "[" ? "]" : QuoteTypes[SelectedQuoteType])} " +
                            $"({headers_text}) VALUES ({row_text});"
                            );

                    SetProgressBarValue(progress_bar, i, 0, rows.LongLength - 1);
                }

                return sql_builder.ToString();
            });
        }
    }
}
