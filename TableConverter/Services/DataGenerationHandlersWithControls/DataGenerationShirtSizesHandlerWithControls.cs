using Avalonia.Controls;
using Avalonia.Layout;
using System.Collections.ObjectModel;
using TableConverter.DataGeneration.DataGenerationHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlersWithControls
{
    public class DataGenerationShirtSizesHandlerWithControls : DataGenerationShirtSizesHandler, IInitializeControls
    {
        public Collection<Control> OptionsControls { get; set; } = new();

        public void InitializeControls()
        {
            var shirt_size_group_label = new TextBlock()
            {
                Text = "Shirt Size Type:",
                VerticalAlignment = VerticalAlignment.Center,
            };

            var shirt_size_group_combo_box = new ComboBox()
            {
                ItemsSource = Options!.ShirtSizeGroups.Keys,
                SelectedIndex = 0,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            shirt_size_group_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string item)
                {
                    Options!.ShirtSizeGroup = item;
                }
            };

            OptionsControls.Add(shirt_size_group_label);
            OptionsControls.Add(shirt_size_group_combo_box);
        }
    }
}
