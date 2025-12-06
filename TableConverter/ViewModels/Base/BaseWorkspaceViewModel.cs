using System;
using System.Collections.ObjectModel;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;

namespace TableConverter.ViewModels.Base;

public abstract partial class BaseWorkspaceViewModel : BaseViewModel, IWorkspace
{
    #region Properties
    
    [ObservableProperty] private string _Title;
    [ObservableProperty] private object _Icon;
    [ObservableProperty] private int _Index;
    [ObservableProperty] private bool _IsBusy;
    [ObservableProperty] private string _BusyText;
    [ObservableProperty] private ObservableCollection<ICommandInstance> _MainCommands;

    #endregion

    #region Constructors

    protected BaseWorkspaceViewModel(
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
        BusyText = string.Empty;
        IsBusy = false;
        MainCommands = [];
    }

    #endregion
    
    #region Methods
    
    public void SetBusy(bool isBusy, string busyText = "Loading...")
    {
        IsBusy = isBusy;
        BusyText = busyText;
    }
    
    public void ClearBusy()
    {
        IsBusy = false;
        BusyText = string.Empty;
    }
    
    #endregion
}

