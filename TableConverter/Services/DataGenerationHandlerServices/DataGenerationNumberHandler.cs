using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlerServices
{
    internal class DataGenerationNumberHandler : DataGenerationTypeHandlerAbstract
    {
        private long MinValue { get; set; } = 1;

        private long MaxValue { get; set; } = 100;

        private short DecimalPlaces { get; set; } = 0;

        public override void InitializeOptionsControls()
        { 
            var min_label = new Label()
            {
                Content = "Min:",
                Margin = new Thickness(5, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            var min_numeric_up_down = new NumericUpDown()
            {
                Minimum = long.MinValue,
                Maximum = long.MaxValue - 1,
                Value = MinValue,
                ParsingNumberStyle = NumberStyles.Integer,
                FormatString = "N0",
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            min_numeric_up_down.ValueChanged += (sender, e) =>
            {
                if (sender is NumericUpDown numeric_up_down)
                {
                    MinValue = (long)(numeric_up_down.Value ?? long.MinValue) ;
                }
            };

            OptionsControls.Add(min_label);
            OptionsControls.Add(min_numeric_up_down);

            var max_label = new Label()
            {
                Content = "Max:",
                Margin = new Thickness(5, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            var max_numeric_up_down = new NumericUpDown()
            {
                Minimum = long.MinValue + 1,
                Maximum = long.MaxValue,
                Value = MaxValue,
                ParsingNumberStyle = NumberStyles.Integer,
                FormatString = "N0",
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            max_numeric_up_down.ValueChanged += (sender, e) =>
            {
                if (sender is NumericUpDown numeric_up_down)
                {
                    MaxValue = (long)(numeric_up_down.Value ?? long.MaxValue);
                }
            };

            OptionsControls.Add(max_label);
            OptionsControls.Add(max_numeric_up_down);

            var decimal_label = new Label()
            {
                Content = "Decimal Places:",
                Margin = new Thickness(5, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            var decimal_numeric_up_down = new NumericUpDown()
            {
                Minimum = 0,
                Maximum = 9,
                Value = DecimalPlaces,
                ParsingNumberStyle = NumberStyles.Integer,
                FormatString = "N0",
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            decimal_numeric_up_down.ValueChanged += (sender, e) =>
            {
                if (sender is NumericUpDown numeric_up_down)
                {
                    DecimalPlaces = (short)(numeric_up_down.Value ?? 0);
                }
            };

            OptionsControls.Add(decimal_label);
            OptionsControls.Add(decimal_numeric_up_down);
        }

        public override Task<string[]> GenerateData(long rows, int blanks_percentage)
        {
            return Task.Run(() =>
            {
                List<string> data = new List<string>();

                for (long i = 0; i < rows; i++)
                {
                    data.Add(CheckBlank(() => (Random.NextInt64(MinValue, MaxValue) + Random.NextDouble()).ToString($"N{DecimalPlaces}"), blanks_percentage));
                }

                return data.ToArray();
            });
        }
    }
}
