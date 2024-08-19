using Avalonia.Controls;
using Avalonia.Layout;
using System;
using System.IO;
using System.Threading.Tasks;
using TableConverter.DataModels;

namespace TableConverter.Services.ConverterHandlerServices
{
    internal class ConverterHandlerMultiLineOutputService : ConverterHandlerOutputAbstract
    {
        private string RowSeparator { get; set; } = "---";

        public override void InitializeControls()
        {
            Controls = new();

            var row_separator_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var row_separator_label = new TextBlock()
            {
                Text = "Row Separator:",
            };

            var row_separator_text_box = new TextBox()
            {
                Text = RowSeparator,
            };

            row_separator_text_box.TextChanged += (sender, e) =>
            {
                if (sender is TextBox text_box)
                {
                    RowSeparator = text_box.Text ?? "";
                }
            };

            row_separator_stack_panel.Children.Add(row_separator_label);
            row_separator_stack_panel.Children.Add(row_separator_text_box);

            Controls?.Add(row_separator_stack_panel);
        }

        public override Task<string> ConvertAsync(string[] headers, string[][] rows)
        {
            return Task.Run(() =>
            {
                using (var writer = new StringWriter())
                {
                    foreach (string column in headers)
                    {
                        writer.WriteLine(column);
                    }

                    for (long i = 0; i < rows.LongLength; i++)
                    {
                        if (RowSeparator != string.Empty)
                        {
                            writer.WriteLine(RowSeparator);
                        }

                        for (long j = 0; j < headers.LongLength; j++)
                        {
                            writer.WriteLine(rows[i][j]);
                        }
                    }

                    return writer.ToString();
                }
            });
        }
    }
}
