using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TableConverter.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    #region Commands

    [RelayCommand]
    protected void LightDarkModeButtonClicked()
    {
        App.ThemeManager?.Switch(App.Current?.ActualThemeVariant.ToString() == "Dark" ? 0 : 1);
    }

    #endregion
}
