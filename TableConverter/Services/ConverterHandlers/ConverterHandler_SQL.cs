using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TableConverter.Services.ConverterHandlers
{
    public class ConverterHandler_SQL : ConverterHandlerBase
    {
        private string[] QuoteTypes { get; init; } = { "No Quotes", "Double Quotes", "MySQL Quotes", "SQLite/Server Quotes" };

        private string CurrentQuoteType { get; set; } = "No Quotes";
        private string TableName { get; set; } = "";

        public override void InitializeControls()
        {
            base.InitializeControls();

            var QuoteTypeStackPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };

            var QuoteTypeLabel = new Label { Content = "Quote Type:" };

            var QuoteTypeComboBox = new ComboBox
            {
                ItemsSource = QuoteTypes,
                SelectedIndex = 0
            };

            QuoteTypeComboBox.SelectionChanged += (sender, e) =>
            {
                if (e.AddedItems.Count > 0)
                {
                    CurrentQuoteType = e.AddedItems[0].ToString();
                }
            };

            QuoteTypeStackPanel.Children.Add(QuoteTypeLabel);
            QuoteTypeStackPanel.Children.Add(QuoteTypeComboBox);

            var TableNameStackPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };

            var TableNameLabel = new Label { Content = "Table Name:" };

            var TableNameTextBox = new TextBox
            {
                Text = TableName
            };

            TableNameTextBox.TextChanged += (sender, e) =>
            {
                if (sender is TextBox)
                {
                    TableName = ((TextBox)sender).Text;
                }
            };

            TableNameStackPanel.Children.Add(TableNameLabel);
            TableNameStackPanel.Children.Add(TableNameTextBox);

            Controls?.Add(QuoteTypeStackPanel);
            Controls?.Add(TableNameStackPanel);
        }

        public override Task<DataTable> ConvertAsync(IStorageFile input)
        {
            return Task.Run(async () =>
            {
                DataTable data_table = new DataTable();

                try
                {
                    using var reader = new StreamReader(await input.OpenReadAsync());

                    while (!reader.EndOfStream)
                    {
                        string line = await reader.ReadLineAsync();

                        // Checks if the line is an INSERT INTO statement
                        if (line.StartsWith("INSERT INTO"))
                        {
                            // Gets the table name from the INSERT INTO statement
                            string temp_table_name = Regex.Match(line, @"(?<=INSERT INTO )\w+").Value;

                            // Gets the column names from the INSERT INTO statement
                            string[] names = Regex.Match(line, @"(?<=\()(.+?)(?=\))").Value.Split(',');

                            if (data_table.Columns.Count == 0)
                            {
                                foreach (string value in names)
                                {
                                    string temp_column = value.Trim();

                                    if ((temp_column.StartsWith('\"') && temp_column.EndsWith('\"')) ||
                                        (temp_column.StartsWith('[') && temp_column.EndsWith(']')) ||
                                        (temp_column.StartsWith('`') && temp_column.EndsWith('`')))
                                    {
                                        temp_column = temp_column.Substring(1, temp_column.Length - 2);
                                    }

                                    data_table.Columns.Add(temp_column);
                                }
                            }

                            // Gets the values from the INSERT INTO statement
                            string[] values = Regex.Match(line, @"(?<=VALUES \()(.*?)(?=\))").Value.Split(',');

                            DataRow row = data_table.NewRow();

                            for (int i = 0; i < values.Length; i++)
                            {
                                string temp = values[i].Trim();
                                row[i] = temp.Substring(1, temp.Length - 2);
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

        public override Task<string> ConvertAsync(DataTable input, ProgressBar progress_bar)
        {
            return Task.Run(() =>
            {
                string output = string.Empty;

                for (int i = 0; i < input.Rows.Count; ++i)
                {
                    output += $"INSERT INTO {GetQuote()}{TableName.Replace(' ', '_')}{(GetQuote() == "[" ? "]" : GetQuote())} (";
                    
                    foreach (DataColumn column in input.Columns)
                    {
                        output += $"{GetQuote()}{column.ColumnName.Replace(' ', '_')}{(GetQuote() == "[" ? "]" : GetQuote())}";

                        if (column.Ordinal != input.Columns.Count - 1)
                        {
                            output += ", ";
                        }
                    }

                    output += ") VALUES (";

                    foreach (var value in input.Rows[i].ItemArray)
                    {
                        output += $"'{value}'";

                        if (value != input.Rows[i].ItemArray.Last())
                        {
                            output += ", ";
                        }
                    }

                    output += $");{Environment.NewLine}";

                    Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, input.Rows.Count - 1, 0, 1000));
                }

                return output;
            });
        }

        private string GetQuote()
        {
            return CurrentQuoteType switch
            {
                "No Quotes" => "",
                "Double Quotes" => "\"",
                "MySQL Quotes" => "`",
                "SQLite/Server Quotes" => "[",
                _ => "",
            };
        }
    }
}
