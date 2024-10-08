﻿using Avalonia.Controls;
using Avalonia.Layout;
using System.Collections.ObjectModel;
using System.Globalization;
using TableConverter.DataGeneration.DataGenerationHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlersWithControls
{
    public class DataGenerationNumberHandlerWithControls : DataGenerationNumberHandler, IInitializeControls
    {
        public Collection<Control> Controls { get; set; } = new();

        public void InitializeControls()
        {
            Controls.Clear();
            
            var min_label = new TextBlock()
            {
                Text = "Min:",
                VerticalAlignment = VerticalAlignment.Center,
            };

            var min_numeric_up_down = new NumericUpDown()
            {
                Minimum = long.MinValue,
                Maximum = long.MaxValue - 1,
                Value = Options!.MinValue,
                ParsingNumberStyle = NumberStyles.Integer,
                FormatString = "N0",
            };

            min_numeric_up_down.ValueChanged += (sender, e) =>
            {
                if (sender is NumericUpDown numeric_up_down)
                {
                    Options!.MinValue = (long)(numeric_up_down.Value ?? long.MinValue);
                }
            };

            Controls.Add(min_label);
            Controls.Add(min_numeric_up_down);

            var max_label = new TextBlock()
            {
                Text = "Max:",
                VerticalAlignment = VerticalAlignment.Center,
            };

            var max_numeric_up_down = new NumericUpDown()
            {
                Minimum = long.MinValue + 1,
                Maximum = long.MaxValue,
                Value = Options!.MaxValue,
                ParsingNumberStyle = NumberStyles.Integer,
                FormatString = "N0",
            };

            max_numeric_up_down.ValueChanged += (sender, e) =>
            {
                if (sender is NumericUpDown numeric_up_down)
                {
                    Options!.MaxValue = (long)(numeric_up_down.Value ?? long.MaxValue);
                }
            };

            Controls.Add(max_label);
            Controls.Add(max_numeric_up_down);

            var decimal_label = new TextBlock()
            {
                Text = "Decimal Places:",
                VerticalAlignment = VerticalAlignment.Center,
            };

            var decimal_numeric_up_down = new NumericUpDown()
            {
                Minimum = 0,
                Maximum = 9,
                Value = Options!.DecimalPlaces,
                ParsingNumberStyle = NumberStyles.Integer,
                FormatString = "N0",
            };

            decimal_numeric_up_down.ValueChanged += (sender, e) =>
            {
                if (sender is NumericUpDown numeric_up_down)
                {
                    Options!.DecimalPlaces = (short)(numeric_up_down.Value ?? 0);
                }
            };

            Controls.Add(decimal_label);
            Controls.Add(decimal_numeric_up_down);
        }
    }
}
