using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI;
using SukiUI.Dialogs;
using SukiUI.Models;
using SukiUI.Toasts;
using System.Collections.Generic;
using System.Linq;
using TableConverter.Services;

namespace TableConverter.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    #region Services

    public ISukiToastManager ToastManager { get; }

    public ISukiDialogManager DialogManager { get; }

    public SukiTheme Theme { get; }

    #endregion

    #region Properties

    public IAvaloniaReadOnlyList<BasePageViewModel> Pages { get; }

    [ObservableProperty]
    private BasePageViewModel? _SelectedPage = null;

    [ObservableProperty]
    private SukiColorTheme? _SelectedColorTheme = null;

    [ObservableProperty]
    private ThemeVariant? _SelectedBaseTheme = null;

    #endregion

    #region Constructors

    public MainWindowViewModel(IEnumerable<BasePageViewModel> pages, PageNavigationService pageNavigation, ISukiDialogManager dialogManager, ISukiToastManager toastManager)
    {
        Pages = new AvaloniaList<BasePageViewModel>(pages.OrderBy(val => val.Index).ThenBy(val => val.DisplayName));

        DialogManager = dialogManager;
        ToastManager = toastManager;

        Theme = SukiTheme.GetInstance();

        pageNavigation.NavigationRequested += (pageType, action) =>
        {
            var page = Pages.FirstOrDefault(x => x.GetType() == pageType);

            if (page is null || SelectedPage?.GetType() == pageType)
            {
                return;
            }

            SelectedPage = page;

            action?.Invoke(page);
        };

        SelectedColorTheme = Theme.ActiveColorTheme;
        SelectedBaseTheme = Theme.ActiveBaseTheme;
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void ToggleBaseTheme() => Theme.SwitchBaseTheme();

    #endregion

    #region Misc Items

    partial void OnSelectedColorThemeChanged(SukiColorTheme? value) => Theme.ChangeColorTheme(value ?? Theme.ColorThemes.First());

    #endregion
}
