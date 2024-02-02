using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
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
        private string ProcedureName { get; set; } = "";
        private bool GererateProcedure { get; set; } = false;

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

            var GenerateProcedureStackPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };

            var GenerateProcedureLabel = new Label { Content = "Generate Procedure:" };

            var GenerateProcedureCheckBox = new CheckBox { Content = "Generate" };

            GenerateProcedureCheckBox.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox)
                {
                    GererateProcedure = ((CheckBox)sender).IsChecked.Value;
                }
            };

            GenerateProcedureStackPanel.Children.Add(GenerateProcedureLabel);
            GenerateProcedureStackPanel.Children.Add(GenerateProcedureCheckBox);

            var ProcedureNameStackPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };

            var ProcedureNameLabel = new Label { Content = "Procedure Name:" };

            var ProcedureNameTextBox = new TextBox
            {
                Text = ProcedureName
            };

            ProcedureNameTextBox.TextChanged += (sender, e) =>
            {
                if (sender is TextBox)
                {
                    ProcedureName = ((TextBox)sender).Text;
                }
            };

            ProcedureNameStackPanel.Children.Add(ProcedureNameLabel);
            ProcedureNameStackPanel.Children.Add(ProcedureNameTextBox);

            Controls?.Add(QuoteTypeStackPanel);
            Controls?.Add(TableNameStackPanel);
            Controls?.Add(GenerateProcedureStackPanel);
            Controls?.Add(ProcedureNameStackPanel);
        }

        public override Task<(List<string>, List<string[]>)> ConvertAsync(IStorageFile input)
        {
            return Task.Run(async () =>
            {
                List<string> column_values = new List<string>();
                List<string[]> row_values = new List<string[]>();

                try
                {
                    bool first_line = true;
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

                            if (first_line)
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

                                    column_values.Add(temp_column);
                                }
                            }

                            // Gets the values from the INSERT INTO statement
                            string[] values = Regex.Match(line, @"(?<=VALUES \()(.*?)(?=\))").Value.Split(',');

                            for (int i = 0; i < values.Length; i++)
                            {
                                string temp = values[i].Trim();
                                values[i] = temp.Substring(1, temp.Length - 2);
                            }

                            row_values.Add(values);
                        }
                    }   
                }
                catch (Exception)
                {
                    return (new List<string>(), new List<string[]>());
                }

                return (column_values, row_values);
            });
        }

        public override Task<string> ConvertAsync(string[] column_values, string[][] row_values, ProgressBar progress_bar)
        {
            return Task.Run(() =>
            {
                string output = string.Empty;

                if (GererateProcedure)
                {
                    for (int i = 0; i < row_values.Length; ++i)
                    {
                        output += $"{ProcedureName.Replace(' ', '_')}(";

                        foreach (string value in row_values[i])
                        {
                            output += $"'{value}'";

                            if (value != row_values[i].Last())
                            {
                                output += ", ";
                            }
                        }

                        output += $");{Environment.NewLine}";

                        Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, row_values.Length - 1, 0, 100));
                    }
                }
                else
                {
                    for (int i = 0; i < row_values.Length; ++i)
                    {
                        output += $"INSERT INTO {GetQuote()}{TableName.Replace(' ', '_')}{(GetQuote() == "[" ? "]" : GetQuote())} (";

                        foreach (string column in column_values)
                        {
                            output += $"{GetQuote()}{column.Replace(' ', '_')}{(GetQuote() == "[" ? "]" : GetQuote())}";

                            if (column != column_values.Last())
                            {
                                output += ", ";
                            }
                        }

                        output += ") VALUES (";

                        foreach (string value in row_values[i])
                        {
                            output += $"'{value}'";

                            if (value != row_values[i].Last())
                            {
                                output += ", ";
                            }
                        }

                        output += $");{Environment.NewLine}";

                        Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, row_values.Length - 1, 0, 100));
                    }
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
