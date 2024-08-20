using Avalonia;
using Avalonia.Controls;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using TableConverter.DataGeneration.DataGenerationHandlers;
using TableConverter.Interfaces;
using Avalonia.Layout;
using System.Linq;

namespace TableConverter.Services.DataGenerationHandlersWithControls
{
    public class DataGenerationMobileBrandsModelsHandlerWithControls : DataGenerationMobileBrandsModelsHandler, IInitializeControls
    {
        public Collection<Control> Controls { get; set; } = new();

        public void InitializeControls()
        {
            var mobile_brands_label = new TextBlock()
            {
                Text = "Mobile Brand:",
                VerticalAlignment = VerticalAlignment.Center,
            };

            using (var reader = DbConnection.ExecuteCommand(
                "SELECT DISTINCT NAME FROM MOBILE_BRANDS_TABLE;"
            ))
            {
                if (reader.HasRows)
                {
                    var mobile_brands = new List<string>();

                    while (reader.Read())
                    {
                        mobile_brands.Add(reader.GetString(0));
                    }

                    mobile_brands.Sort();

                    Options!.MobileBrands = mobile_brands.ToArray();

                    Options!.MobileBrand = Options!.MobileBrands.First();
                }
            }

            var mobile_brands_combo_box = new ComboBox()
            {
                ItemsSource = Options!.MobileBrands,
                SelectedIndex = 0,
                VerticalAlignment = VerticalAlignment.Center,
            };

            mobile_brands_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string item)
                {
                    Options!.MobileBrand = item;
                }
            };

            Controls.Add(mobile_brands_label);
            Controls.Add(mobile_brands_combo_box);
        }
    }
}
