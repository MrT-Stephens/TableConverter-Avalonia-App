using System;
using System.Collections.Generic;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;

namespace TableConverter.ViewModels;

public partial class BaseWorkspaceViewModel : BaseViewModel, IWorkspace
{
    #region Properties

    [ObservableProperty] private string _Header;
    [ObservableProperty] private object _Icon;
    [ObservableProperty] private int _Index;
    [ObservableProperty] private IEnumerable<IPane> _Panes =
    [
    ];

    #endregion
    
    #region Constructors

    public BaseWorkspaceViewModel(
        ICommandManager commandManager,
        IEventManager eventManager,
        ISukiDialogManager dialogManager,
        ISukiToastManager toastManager,
        string header,
        string icon,
        int index = 0)
        : base(commandManager, eventManager, dialogManager, toastManager)
    {
        _Icon = Application.Current?.Resources[icon]
            ?? throw new NullReferenceException("Icon not found");
        
        _Header = header;
        _Index = index;
    }
    
    #endregion
}