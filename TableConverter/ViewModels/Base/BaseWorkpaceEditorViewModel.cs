using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;
using System.Collections.ObjectModel;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;

namespace TableConverter.ViewModels.Base;

public partial class BaseWorkpaceEditorViewModel<TViewModel> : BaseViewModel, IWorkspaceEditor<TViewModel>
    where TViewModel : IWorkspace
{
    #region Properties

    [ObservableProperty] private string _Title;
    [ObservableProperty] private object _Icon;
    [ObservableProperty] private int _Index;

    [ObservableProperty] private ObservableCollection<IPaneDocument<TViewModel>> _Documents;
    [ObservableProperty] private ObservableCollection<IPaneTool<TViewModel>> _Tools;

    #endregion

    #region Constructors

    public BaseWorkpaceEditorViewModel(
        ICommandManager commandManager,
        IEventManager eventManager,
        ISukiDialogManager dialogManager,
        ISukiToastManager toastManager,
        string title,
        string iconPath,
        int index = 0)
        : base(commandManager, eventManager, dialogManager, toastManager)
    {
        Title = title;
        Index = index;

        Icon = Application.Current!.Resources[iconPath]
            ?? throw new ArgumentNullException(nameof(iconPath), $"Icon resource '{iconPath}' not found.");
    }

    #endregion
}