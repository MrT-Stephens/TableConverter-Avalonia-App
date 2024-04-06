using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services
{
    public class ConverterHandlerCsvInputService : ConverterHandlerInputAbstract
    {
        private string Delimiter = ",";

        public override void InitializeControls()
        {
            var delimiter_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            var delimiter_label = new Label()
            {
                Content = "Delimiter:",
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            var delimiter_text_box = new TextBox()
            {
                Text = Delimiter,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            delimiter_text_box.TextChanged += (sender, e) =>
            {
                if (sender is TextBox text_box && !string.IsNullOrEmpty(text_box.Text))
                {
                    text_box.Text = Delimiter = text_box.Text!.Substring(0, 1);
                }
            };

            delimiter_stack_panel.Children.Add(delimiter_label);
            delimiter_stack_panel.Children.Add(delimiter_text_box);

            Controls?.Add(delimiter_stack_panel);
        }

        public override Task<(List<string>, List<string[]>)> ReadTextAsync(string text)
        {
            return Task.Run(() =>
            {
                var headers = new List<string>();
                var rows = new List<string[]>();

                var lines = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

                if (lines.Length > 0)
                {
                    headers = lines[0].Split(Delimiter).ToList();

                    for (int i = 1; i < lines.Length; i++)
                    {
                        rows.Add(lines[i].Split(Delimiter));
                    }
                }

                return (headers, rows);
            }); 
        }
    }
}
