using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Data;
using System.Threading.Tasks;

namespace TableConverter.Services.ConverterHandlers
{
    public class ConverterHandler_MultiLine : ConverterHandlerBase
    {
        private string RowSeparator { get; set; } = "";

        public override void InitializeControls()
        {
            base.InitializeControls();

            var SeparatorStackPanel = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Vertical };

            var SeparatorLabel = new Label() { Content = "Row Separator:" };

            var SeparatorTextBox = new TextBox() { Text = RowSeparator };

            SeparatorTextBox.TextChanged += (sender, e) =>
            {
                if (sender is TextBox)
                {
                    RowSeparator = ((TextBox)sender).Text;
                }
            };

            SeparatorStackPanel.Children.Add(SeparatorLabel);
            SeparatorStackPanel.Children.Add(SeparatorTextBox);

            Controls?.Add(SeparatorStackPanel);
        }

        public override Task<string> ConvertAsync(string[] column_values, string[][] row_values, ProgressBar progress_bar)
        {
            return Task.Run(() =>
            {
                string output = string.Empty;

                foreach (string column in column_values)
                {
                    output += column + Environment.NewLine;
                }

                for (int i = 0; i < row_values.Length; i++)
                {
                    if (RowSeparator != string.Empty)
                    {
                        output += RowSeparator + Environment.NewLine;
                    }

                    for (int j = 0; j < column_values.Length; j++)
                    {
                        output += row_values[i][j] + Environment.NewLine;
                    }

                    Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, row_values.Length - 1, 0, 100));
                }

                return output;
            });
        }
    }
}
