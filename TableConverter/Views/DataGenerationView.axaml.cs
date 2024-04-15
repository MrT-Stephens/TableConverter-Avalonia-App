using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TableConverter.Views
{
    public partial class DataGenerationView : UserControl
    {
        public DataGenerationView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            InitializeComponent(true);
        }

        private void HeaderMenuLightDarkModeButtonClicked(object? sender, RoutedEventArgs e)
        {
            App.ThemeManager?.Switch(App.Current?.ActualThemeVariant.ToString() == "Dark" ? 0 : 1);
        }
    }
}
