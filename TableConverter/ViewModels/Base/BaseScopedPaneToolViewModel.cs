using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;

namespace TableConverter.ViewModels.Base;

public abstract partial class BaseScopedPaneToolViewModel<TWorkspace> : BaseViewModel, IScopedPaneTool<TWorkspace>
{
    #region Properties

    [ObservableProperty] private string _Title;
    [ObservableProperty] private bool _IsEnabled;
    [ObservableProperty] private object _Workspace;
    [ObservableProperty] private object? _SelectedItem;

    #endregion

    #region Constructors

    protected BaseScopedPaneToolViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager,
        ISukiDialogManager dialogManager,
        ISukiToastManager  toastManager,
        string title)
        : base(commandManager, eventManager, dialogManager, toastManager)
    {
        Title = title;
        IsEnabled = true;
        Workspace = null!;
    }

    #endregion

    #region Methods

    public bool TryGetSelectedItem<T>([NotNullWhen(true)] out T? item)
    {
        if (SelectedItem is T castItem)
        {
            item = castItem;
            return true;
        }

        item = default;
        return false;
    }

    #endregion
}