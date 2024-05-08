using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using MathNet.Numerics.Random;
using System;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlerServices
{
    internal class DataGenerationGenderHandler : DataGenerationTypeHandlerAbstract
    {
        private readonly string[] GenderFormats = 
        [
            "Male",
            "M",
            "male",
            "m"
        ];

        private string GenderFormat { get; set; } = "Male";

        public override void InitializeOptionsControls()
        {
            var format_label = new Label()
            {
                Content = "Format:",
                Margin = new Thickness(5, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            var gender_format_combo_box = new ComboBox()
            {
                ItemsSource = GenderFormats,
                SelectedIndex = 0,
                VerticalAlignment = VerticalAlignment.Stretch,
                MinWidth = 100,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            gender_format_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string item)
                {
                    GenderFormat = item;
                }
            };

            OptionsControls.Add(format_label);
            OptionsControls.Add(gender_format_combo_box);
        }

        public override Task<string[]> GenerateData(long rows, int blanks_percentage)
        {
            return Task.Run(() =>
            {
                string[] data = new string[rows];

                for (int i = 0; i < rows; i++)
                {
                    data[i] = CheckBlank(() =>
                    {
                        return GenderFormat switch
                        {
                            "M" => Random.NextBoolean() ? "M" : "F",
                            "m" => Random.NextBoolean() ? "m" : "f",
                            "male" => Random.NextBoolean() ? "male" : "female",
                            "Male" or _ => Random.NextBoolean() ? "Male" : "Female"
                        };
                    }, blanks_percentage);
                }

                return data;
            });
        }
    }
}
