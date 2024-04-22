using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlerServices
{
    internal class DataGenerationIPAddressHandler : DataGenerationTypeHandlerAbstract
    {
        private readonly string[] IpTypes =
        [
            "IPv4", 
            "IPv6" 
        ];

        private string SelectedIpType { get; set; } = "IPv4";

        public override void InitializeOptionsControls()
        {
            var ip_type_combo_box = new ComboBox()
            {
                ItemsSource = IpTypes,
                SelectedItem = SelectedIpType,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                MinWidth = 200,
            };

            ip_type_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_type)
                {
                    SelectedIpType = selected_type;
                }
            };

            OptionsControls.Add(ip_type_combo_box);
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
                        switch (SelectedIpType)
                        {
                            default:
                            case "IPv4":
                                {
                                    return $"{new Random().Next(0, 256)}.{new Random().Next(0, 256)}.{new Random().Next(0, 256)}.{new Random().Next(0, 256)}";
                                }
                            case "IPv6":
                                {
                                    string[] groups = new string[8];
                                    for (int i = 0; i < 8; i++)
                                    {
                                        groups[i] = Random.Next(65536).ToString("X4");
                                    }

                                    return string.Join(":", groups);
                                }
                        }
                    }
                    , blanks_percentage);
                }

                return data;
            });
        }
    }
}
