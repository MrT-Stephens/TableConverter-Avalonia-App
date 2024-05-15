using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlerServices
{
    internal class DataGenerationShirtSizesHandler : DataGenerationTypeHandlerAbstract
    {
        private readonly Dictionary<string, string[]> ShirtSizeGroups = new Dictionary<string, string[]>()
        {
            { "Men's", [ "XS", "S", "M", "L", "XL", "XXL", "XXXL", "XXXXL" ] }, 
            { "Women's Normal", [ "XS", "S", "M", "L", "XL", "XXL", "XXXL" ] }, 
            { "Women's Numeric", [ "Size 6", "Size 8", "Size 10", "Size 12", "Size 14", "Size 16", "Size 18" ] }, 
            { "Children's", [ "Newborn (NB)", "0-3 months", "3-6 months", "6-9 months", "9-12 months", "12-18 months", "18-24 months", "2T", "3T", "4T", "5T" ] },
            { "All", [] }
        };

        private string ShirtSizeGroup { get; set; } = string.Empty;

        public override void InitializeOptionsControls()
        {
            var shirt_size_group_label = new Label()
            {
                Content = "Shirt Size Type:",
                Margin = new Thickness(5, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            var shirt_size_group_combo_box = new ComboBox()
            {
                ItemsSource = ShirtSizeGroups.Keys,
                SelectedIndex = 0,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            shirt_size_group_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string item)
                {
                    ShirtSizeGroup = item;
                }
            };

            OptionsControls.Add(shirt_size_group_label);
            OptionsControls.Add(shirt_size_group_combo_box);
        }

        public override Task<string[]> GenerateData(long rows, int blanks_percentage)
        {
            return Task.Run(() =>
            {
                string[] data = new string[rows];

                string[] sizes = (ShirtSizeGroup == "All") ? 
                    ShirtSizeGroups.Values.SelectMany(x => x).ToArray() :
                    ShirtSizeGroups[ShirtSizeGroup];

                for (int i = 0; i < rows; i++)
                {
                    data[i] = (CheckBlank(() => sizes[Random.Next(0, sizes.Length - 1)], blanks_percentage));
                }

                return data;
            });
        }
    }
}
