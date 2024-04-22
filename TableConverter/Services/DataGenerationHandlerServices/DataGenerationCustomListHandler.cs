using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlerServices
{
    internal class DataGenerationCustomListHandler : DataGenerationTypeHandlerAbstract
    {
        private string ItemsList { get; set; } = string.Empty;

        public override void InitializeOptionsControls()
        {
            var sequence_text_box = new TextBox()
            {
                Watermark = "Example: item 1,item 2,item 3",
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                MinWidth = 200,
            };

            sequence_text_box.TextChanged += (sender, e) =>
            {
                if (sender is TextBox text_box)
                {
                    ItemsList = text_box.Text ?? "";
                }
            };

            OptionsControls.Add(sequence_text_box);
        }

        public override Task<string[]> GenerateData(long rows, int blanks_percentage)
        {
            return Task.Run(() =>
            {
                List<string> data = new List<string>();

                for (int i = 0; i < rows; i++)
                {
                    data.Add(CheckBlank(() =>
                    {
                        string[] items = ItemsList.Split(',');
                        return items[new Random().Next(0, items.Length)];
                    }
                    , blanks_percentage));
                }

                return data.ToArray();
            });
        }
    }
}
