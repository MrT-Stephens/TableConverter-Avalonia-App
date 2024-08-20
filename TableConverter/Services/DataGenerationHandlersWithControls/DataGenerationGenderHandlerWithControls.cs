using Avalonia.Controls;
using Avalonia.Layout;
using System.Collections.ObjectModel;
using TableConverter.DataGeneration.DataGenerationHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlersWithControls
{
    public class DataGenerationGenderHandlerWithControls : DataGenerationGenderHandler, IInitializeControls
    {
        public Collection<Control> Controls { get; set; } = new();

        public void InitializeControls()
        {
            var format_label = new TextBlock()
            {
                Text = "Format:",
                VerticalAlignment = VerticalAlignment.Center,
            };

            var gender_format_combo_box = new ComboBox()
            {
                ItemsSource = Options!.GenderFormats,
                SelectedIndex = 0,
                VerticalAlignment = VerticalAlignment.Center,
                MinWidth = 100,
            };

            gender_format_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string item)
                {
                    Options!.GenderFormat = item;
                }
            };

            Controls.Add(format_label);
            Controls.Add(gender_format_combo_box);
        }
    }
}
