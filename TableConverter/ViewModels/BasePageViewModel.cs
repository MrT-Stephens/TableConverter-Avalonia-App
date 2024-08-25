using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;

namespace TableConverter.ViewModels;

public abstract partial class BasePageViewModel : ObservableValidator
{
    protected readonly ISukiDialogManager DialogManager;
    protected readonly ISukiToastManager ToastManager;

    [ObservableProperty]
    private string _DisplayName;

    [ObservableProperty]
    private object _Icon;

    [ObservableProperty]
    private int _Index;

    public BasePageViewModel(ISukiDialogManager dialogManager, ISukiToastManager toastManager, string displayName, object? icon, int index = 0)
    {
        ArgumentNullException.ThrowIfNull(icon, nameof(icon));

        DialogManager = dialogManager;
        ToastManager = toastManager;
        DisplayName = displayName;
        Icon = icon;
        Index = index;
    }
}
