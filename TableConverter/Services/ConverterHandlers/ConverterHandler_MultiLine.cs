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

        public override Task<string> ConvertAsync(DataTable input, ProgressBar progress_bar)
        {
            return Task.Run(() =>
            {
                string output = string.Empty;

                foreach (DataColumn column in input.Columns)
                {
                    output += column.ColumnName + Environment.NewLine;
                }

                for (int i = 0; i < input.Rows.Count; i++)
                {
                    if (RowSeparator != string.Empty)
                    {
                        output += RowSeparator + Environment.NewLine;
                    }

                    for (int j = 0; j < input.Columns.Count; j++)
                    {
                        output += input.Rows[i][j].ToString() + Environment.NewLine;
                    }

                    Dispatcher.UIThread.InvokeAsync(() => progress_bar.Value = MapValue(i, 0, input.Rows.Count - 1, 0, 1000));
                }

                return output;
            });
        }
    }
}
