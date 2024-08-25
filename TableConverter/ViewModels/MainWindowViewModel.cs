using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System.Collections.Generic;
using System.Linq;

namespace TableConverter.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    #region Services

    public ISukiToastManager ToastManager { get; }

    public ISukiDialogManager DialogManager { get; }

    #endregion

    #region Properties

    public IAvaloniaReadOnlyList<BasePageViewModel> Pages { get; }

    [ObservableProperty]
    private BasePageViewModel? _SelectedPage = null;

    #endregion

    #region Constructors

    public MainWindowViewModel(IEnumerable<BasePageViewModel> pages, ISukiDialogManager dialogManager, ISukiToastManager toastManager)
    {
        Pages = new AvaloniaList<BasePageViewModel>(pages.OrderBy(val => val.Index).ThenBy(val => val.DisplayName));

        DialogManager = dialogManager;
        ToastManager = toastManager;
    }

    #endregion
}
