using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TableConverter.DataModels;

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

        public override Task<(List<string>, List<string[]>)> ConvertAsync(IStorageFile input)
        {
            return Task.Run(async () =>
            {
                List<string> column_values = new List<string>();
                List<string[]> row_values = new List<string[]>();

                try
                {
                    using var stream = await input.OpenReadAsync();

                    TextFieldParser parser = new TextFieldParser(stream);
                    {
                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters(new [] { ",", "\t", ";", "|" });

                        bool first_line = true;

                        while (!parser.EndOfData)
                        {
                            string[]? fields = parser.ReadFields();

                            if (first_line && fields != null)
                            {
                                first_line = false;

                                column_values = fields.ToList();
                            }
                            else if (fields != null)
                            {
                                row_values.Add(fields);
                            }
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

                foreach (string column in column_values)
                {
                    output += $"{GetQuote()}{column}{GetQuote()}";

                    if (column != column_values.Last())
                    {
                        output += GetDelimiter();
                    }
                }

                output += "\n";

                for (int i = 0; i < row_values.Length; ++i)
                {
                    foreach (string item in row_values[i])
                    {
                        output += $"{GetQuote()}{item}{GetQuote()}";

                        if (item != row_values[i].Last())
                        {
                            output += GetDelimiter();
                        }
                    }

                    output += Environment.NewLine;

                    Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, row_values.Length - 1, 0, 100));
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
