using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace TableConverter.Services.ConverterHandlers
{
    public class ConverterHandler_YAML : ConverterHandlerBase
    {
        private string[] Quotes { get; init; } = new string[]
        {
            "No Quotes",
            "Single Quotes",
            "Double Quotes"
        };

        private string CurrentQuote { get; set; } = "No Quotes";

        public override void InitializeControls()
        {
            base.InitializeControls();

            var QuoteStackPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };

            var QuoteLabel = new Label { Content = "Quote Type:" };

            var QuoteComboBox = new ComboBox
            {
                ItemsSource = Quotes,
                SelectedIndex = 0
            };

            QuoteComboBox.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox)
                {
                    CurrentQuote = ((ComboBox)sender).SelectedItem.ToString();
                }
            };

            QuoteStackPanel.Children.Add(QuoteLabel);
            QuoteStackPanel.Children.Add(QuoteComboBox);

            Controls?.Add(QuoteStackPanel);
        }

        public override Task<DataTable> ConvertAsync(IStorageFile input)
        {
            return Task.Run(async () =>
            {
                DataTable data_table = new DataTable();

                try
                {
                    using var reader = new StreamReader(await input.OpenReadAsync());

                    string data = (await reader.ReadToEndAsync()).Replace("---", string.Empty);
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
                string output = $"---{Environment.NewLine}";

                var quote = CurrentQuote switch
                {
                    "No Quotes" => "",
                    "Single Quotes" => "'",
                    "Double Quotes" => "\"",
                    _ => ""
                };

                for (int i = 0; i < input.Rows.Count; i++)
                {
                    output += $"-{Environment.NewLine}";

                    for (int j = 0; j < input.Columns.Count; j++)
                    {
                        output += $"\t{input.Columns[j].ColumnName.Replace(' ', '_')}: {quote}{input.Rows[i][j]}{quote}{Environment.NewLine}";
                    }

                    Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, input.Rows.Count - 1, 0, 1000));
                }

                return output;
            });
        }
    }
}
