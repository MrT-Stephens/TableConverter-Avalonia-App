using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Collections;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Contracts.Events;
using TableConverter.Interfaces;
using TableConverter.Utilities.Extensions;

namespace TableConverter.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    #region Properties

    public IAvaloniaReadOnlyList<IWorkspace> Workspaces { get; }
    
    [ObservableProperty] private IWorkspace _SelectedWorkspace;

    #endregion

    #region Constructors
    
    public MainViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager,
        ISukiDialogManager dialogManager,
        ISukiToastManager toastManager,
        IEnumerable<IWorkspace> workspaces) 
        : base(commandManager, eventManager, dialogManager, toastManager)
    {
        Workspaces = new AvaloniaList<IWorkspace>(workspaces
            .OrderBy(w => w.Index)
            .ThenBy(w => w.Header));

        _eventManager
            .GetEvent<PageNavigationRequestedEvent>()
            .Subscribe((_, args) =>
            {
                var workspace = Workspaces.FirstOrDefault(w => w.GetType().Name == args.ViewModelName);

                if (workspace is null || SelectedWorkspace?.GetType().Name == args.ViewModelName)
                {
                    return;
                }
                
                SelectedWorkspace = workspace;

                args.Action?.Invoke(workspace);
            });
    }
    
    #endregion
}
