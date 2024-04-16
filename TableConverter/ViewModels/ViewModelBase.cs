using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TableConverter.Services;

namespace TableConverter.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    #region Services

    protected static readonly PageRouterService PageRouterService = new();

    #endregion

    #region Commands

    [RelayCommand]
    protected void LightDarkModeButtonClicked()
    {
        App.ThemeManager?.Switch(App.Current?.ActualThemeVariant.ToString() == "Dark" ? 0 : 1);
    }

    #endregion
}
