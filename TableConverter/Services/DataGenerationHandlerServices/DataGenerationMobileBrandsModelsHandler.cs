using Avalonia.Controls;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using TableConverter.Interfaces;
using Avalonia.Layout;
using Avalonia.Media;

namespace TableConverter.Services.DataGenerationHandlerServices
{
    internal class DataGenerationMobileBrandsModelsHandler : DataGenerationTypeHandlerAbstract
    {
        private string[]? MobileBrands { get; set; }

        private string? MobileBrand { get; set; }

        public override void InitializeOptionsControls()
        {
            var country_code_label = new Label()
            {
                Content = "Mobile Brand:",
                Margin = new Thickness(5, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            using (var reader = DbConnection.ExecuteCommand(
                "SELECT DISTINCT NAME FROM MOBILE_BRANDS_TABLE;"
            ))
            {
                if (reader.HasRows)
                {
                    var country_codes = new List<string>();

                    while (reader.Read())
                    {
                        country_codes.Add(reader.GetString(0));
                    }

                    country_codes.Sort();

                    MobileBrands = country_codes.ToArray();

                    MobileBrand = MobileBrands.First();
                }
            }

            var country_code_combo_box = new ComboBox()
            {
                ItemsSource = MobileBrands,
                SelectedIndex = 0,
                VerticalAlignment = VerticalAlignment.Stretch,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            country_code_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string item)
                {
                    MobileBrand = item;
                }
            };

            OptionsControls.Add(country_code_label);
            OptionsControls.Add(country_code_combo_box);
        }

        public override Task<string[]> GenerateData(long rows, int blanks_percentage)
        {
            return Task.Run(() =>
            {
                string[] data = new string[rows];

                using (var reader = DbConnection.ExecuteCommand(
                    $"SELECT MM.NAME FROM MOBILE_BRANDS_MODELS_TABLE MM INNER JOIN MOBILE_BRANDS_TABLE MB ON MB.ID = MM.BRAND_ID WHERE MB.NAME = '{MobileBrand}' AND MM.ID IN (SELECT ID FROM MOBILE_BRANDS_MODELS_TABLE WHERE BRAND_ID = MB.ID ORDER BY RANDOM() LIMIT {rows});"
                ))
                {
                    if (!reader.HasRows)
                    {
                        throw new SQLiteException("No rows returned from the database.");
                    }

                    long i = 0;

                    while (reader.Read())
                    {
                        data[i++] = CheckBlank(() => reader.GetString(0), blanks_percentage);
                    }

                    if (i < rows)
                    {
                        for (long j = i; j < rows; j++)
                        {
                            data[j] = data[j - i];
                        }
                    }
                };

                return data;
            });
        }
    }
}
