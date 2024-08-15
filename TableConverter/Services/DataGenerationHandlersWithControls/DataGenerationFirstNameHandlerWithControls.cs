using Avalonia.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TableConverter.DataGeneration.DataGenerationHandlers;
using TableConverter.Interfaces;
using Avalonia.Layout;
using System.Linq;

namespace TableConverter.Services.DataGenerationHandlersWithControls
{
    public class DataGenerationFirstNameHandlerWithControls : DataGenerationFirstNameHandler, IInitializeControls
    {
        public Collection<Control> OptionsControls { get; set; } = new();

        public void InitializeControls()
        {
            var country_code_label = new TextBlock()
            {
                Text = "Country Code:",
                VerticalAlignment = VerticalAlignment.Center,
            };

            using (var reader = DbConnection.ExecuteCommand(
                "SELECT DISTINCT CC.COUNTRY_CODE FROM FIRST_LAST_NAMES_TABLE FL JOIN COUNTRY_CODES_TABLE CC ON FL.COUNTRY_CODE = CC.COUNTRY_CODE;"
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

                    Options!.CountryCodes = country_codes.ToArray();

                    Options!.CountryCode = Options!.CountryCodes.First();
                }
            }

            var country_code_combo_box = new ComboBox()
            {
                ItemsSource = Options!.CountryCodes,
                SelectedIndex = 0,
                VerticalAlignment = VerticalAlignment.Center,
            };

            country_code_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string item)
                {
                    Options!.CountryCode = item;
                }
            };

            OptionsControls.Add(country_code_label);
            OptionsControls.Add(country_code_combo_box);
        }
    }
}
