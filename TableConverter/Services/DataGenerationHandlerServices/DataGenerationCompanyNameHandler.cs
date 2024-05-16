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
    internal class DataGenerationCompanyNameHandler : DataGenerationTypeHandlerAbstract
    {
        private string[]? CountryCodes { get; set; }

        private string? CountryCode { get; set; }

        public override void InitializeOptionsControls()
        {
            var country_code_label = new Label()
            {
                Content = "Country Code:",
                Margin = new Thickness(5, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            using (var reader = DbConnection.ExecuteCommand(
                "SELECT DISTINCT CC.COUNTRY_CODE FROM COMPANIES_TABLE CP JOIN COUNTRY_CODES_TABLE CC ON CP.COUNTRY_CODE = CC.COUNTRY_CODE;"
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

                    CountryCodes = country_codes.ToArray();

                    CountryCode = CountryCodes.First();
                }
            }

            var country_code_combo_box = new ComboBox()
            {
                ItemsSource = CountryCodes,
                SelectedIndex = 0,
                VerticalAlignment = VerticalAlignment.Stretch,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

            country_code_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string item)
                {
                    CountryCode = item;
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
                    $"SELECT CC.COUNTRY_CODE, CP.NAME FROM COMPANIES_TABLE CP INNER JOIN COUNTRY_CODES_TABLE CC ON CC.COUNTRY_CODE = '{CountryCode ?? "GB"}' WHERE CP.ID IN (SELECT ID FROM COMPANIES_TABLE ORDER BY RANDOM() LIMIT {rows});"
                ))
                {
                    if (!reader.HasRows)
                    {
                        throw new SQLiteException("No rows returned from the database.");
                    }

                    long i = 0;

                    while (reader.Read())
                    {
                        data[i++] = CheckBlank(() => reader.GetString(1), blanks_percentage);
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
