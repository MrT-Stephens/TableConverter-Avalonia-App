using Avalonia.Controls;
using Avalonia.Layout;
using System;
using System.IO;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services
{
    internal class ConverterHandlerMultiLineOutputService : ConverterHandlerOutputAbstract
    {
        private string RowSeparator { get; set; } = "---";

        public override void InitializeControls()
        {
            var row_separator_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var row_separator_label = new Label()
            {
                Content = "Row Separator:",
                FontFamily = App.Current?.Resources["JetBrainsMono"] as Avalonia.Media.FontFamily ?? throw new NullReferenceException(),
            };

            var row_separator_text_box = new TextBox()
            {
                Text = RowSeparator,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as Avalonia.Media.FontFamily ?? throw new NullReferenceException(),
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

        public override Task<string> ConvertAsync(string[] headers, string[][] rows, object? progress_bar)
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

                        SetProgressBarValue(progress_bar, i, 0, rows.LongLength - 1);
                    }

                    return writer.ToString();
                }
            });
        }
    }
}
