using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;

namespace TableConverter.ViewModels;

public partial class BasePaneViewModel : BaseViewModel, IPane
{
    #region Properties

    public string Id { get; }
    [ObservableProperty] private string _Header;
    [ObservableProperty] private bool _IsEnabled;
    [ObservableProperty] private bool _IsVisible;
    [ObservableProperty] private ObservableCollection<ICommandMetadata> _ToolbarCommands;
    
    #endregion
    
    #region Constructors

    public BasePaneViewModel(
        string header,
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager,
        ISukiToastManager toastManager) 
        : base(commandManager, eventManager, dialogManager, toastManager)
    {
        Id = Guid.NewGuid().ToString();
        _Header = header;
        _IsEnabled = true;
        _IsVisible = true;
        _ToolbarCommands = [];
    }

    #endregion

    #region Methods
    
    public void AddToolbarCommand(ICommandMetadata command)
    {
#if DEBUG
        if (string.IsNullOrWhiteSpace(command.Name)
            || string.IsNullOrEmpty(command.Description)
            || string.IsNullOrEmpty(command.IconName)
            || string.IsNullOrEmpty(command.Title))
        {
            Debug.Fail("Toolbar command metadata is incomplete.");
        }
#endif
        ToolbarCommands.Add(command);
    }

    #endregion
}