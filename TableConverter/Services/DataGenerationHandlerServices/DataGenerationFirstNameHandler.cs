using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlerServices
{
    internal class DataGenerationFirstNameHandler : DataGenerationTypeHandlerAbstract
    {
        private string[] CountryCodes { get; init; }

        private string CountryCode { get; set; }

        public DataGenerationFirstNameHandler()
        {
            using (var reader = DbConnection.ExecuteCommand(
                "SELECT ID FROM COUNTRY_CODE_TABLE;"
            ))
            {
                if (!reader.HasRows)
                {
                    throw new SQLiteException("No rows returned from the database.");
                }

                var country_codes = new List<string>();

                while (reader.Read())
                {
                    country_codes.Add(reader.GetString(0));
                }

                CountryCodes = country_codes.ToArray();
            }

            CountryCode = CountryCodes.First();
        }

        public override void InitializeOptionsControls()
        {
            var country_code_label = new Label()
            {
                Content = "Country Code:",
                Margin = new Thickness(5, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
            };

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
                    $"SELECT FIRST_NAME FROM FIRST_LAST_NAMES_TABLE WHERE ID IN (SELECT ID FROM FIRST_LAST_NAMES_TABLE ORDER BY RANDOM() LIMIT {rows});"
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
                };

                return data;
            });
        }
    }
}
