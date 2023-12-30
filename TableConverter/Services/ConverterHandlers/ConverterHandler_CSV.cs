using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace TableConverter.Services.ConverterHandlers
{
    public class ConverterHandler_CSV : ConverterHandlerBase
    {
        private string[] DelimiterTypes { get; init; } = { "Comma", "Tab", "Semicolon", "Vertical Bar" };
        private string[] QuoteTypes { get; init; } = { "No Quotes", "Double Quotes", "Single Quotes" };

        private string CurrentDelimiterType { get; set; } = ",";
        private string CurrentQuoteType { get; set; } = "No Quotes";

        public override void InitializeControls()
        {
            base.InitializeControls();

            var DelimiterTypeStackPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };

            var DelimiterTypeLabel = new Label { Content = "Delimiter Type:" };

            var DelimiterTypeComboBox = new ComboBox
            {
                ItemsSource = DelimiterTypes,
                SelectedIndex = 0
            };

            DelimiterTypeComboBox.SelectionChanged += (sender, e) =>
            {
                if (e.AddedItems.Count > 0)
                {
                    CurrentDelimiterType = e.AddedItems[0].ToString();
                }
            };

            DelimiterTypeStackPanel.Children.Add(DelimiterTypeLabel);
            DelimiterTypeStackPanel.Children.Add(DelimiterTypeComboBox);

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

            Controls?.Add(DelimiterTypeStackPanel);
            Controls?.Add(QuoteTypeStackPanel);
        }

        public override Task<DataTable> ConvertAsync(IStorageFile input)
        {
            return Task.Run(async () =>
            {
                DataTable data_table = new DataTable();

                try
                {
                    using var stream = await input.OpenReadAsync();

                    TextFieldParser parser = new TextFieldParser(stream);
                    {
                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters(new string[] { ",", "\t", ";", "|" });

                        bool first_line = true;

                        while (!parser.EndOfData)
                        {
                            string[]? fields = parser.ReadFields();

                            if (first_line && fields != null)
                            {
                                foreach (var value in fields)
                                {
                                    data_table.Columns.Add(value);
                                }

                                first_line = false;
                            }
                            else if (fields != null)
                            {
                                data_table.Rows.Add(fields.ToArray<object>());
                            }
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

                foreach (DataColumn column in input.Columns)
                {
                    output += $"{GetQuote()}{column.ColumnName}{GetQuote()}";

                    if (column.Ordinal != input.Columns.Count - 1)
                    {
                        output += GetDelimiter();
                    }
                }

                output += "\n";

                for (int i = 0; i < input.Rows.Count; ++i)
                {
                    foreach (var item in input.Rows[i].ItemArray)
                    {
                        output += $"{GetQuote()}{item?.ToString()}{GetQuote()}";

                        if (item != input.Rows[i].ItemArray.Last())
                        {
                            output += GetDelimiter();
                        }
                    }

                    output += Environment.NewLine;

                    Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, input.Rows.Count - 1, 0, 1000));
                }

                return output;
            });
        }

        private string GetDelimiter()
        {
            return CurrentDelimiterType switch
            {
                "Comma" => ",",
                "Tab" => "\t",
                "Semicolon" => ";",
                "Vertical Bar" => "|",
                _ => ","
            };
        }

        private string GetQuote()
        {
            return CurrentQuoteType switch
            {
                "No Quotes" => "",
                "Double Quotes" => "\"",
                "Single Quotes" => "'",
                _ => ""
            };
        }
    }
}
