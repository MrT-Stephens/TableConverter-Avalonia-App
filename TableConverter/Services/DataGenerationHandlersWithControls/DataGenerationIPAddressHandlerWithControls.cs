using Avalonia.Controls;
using System.Collections.ObjectModel;
using TableConverter.DataGeneration.DataGenerationHandlers;
using TableConverter.Interfaces;

namespace TableConverter.Services.DataGenerationHandlersWithControls
{
    public class DataGenerationIPAddressHandlerWithControls : DataGenerationIPAddressHandler, IInitializeControls
    {
        public Collection<Control> Controls { get; set; } = new();

        public void InitializeControls()
        {
            Controls.Clear();
            
            var ip_type_combo_box = new ComboBox()
            {
                ItemsSource = Options!.IpTypes,
                SelectedItem = Options!.SelectedIpType,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                MinWidth = 200,
            };

            ip_type_combo_box.SelectionChanged += (sender, e) =>
            {
                if (sender is ComboBox combo_box && combo_box.SelectedItem is string selected_type)
                {
                    Options!.SelectedIpType = selected_type;
                }
            };

            Controls.Add(ip_type_combo_box);
        }
    }
}
