using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Contracts.Events;
using TableConverter.Interfaces;
using TableConverter.ViewModels.Base;

namespace TableConverter.ViewModels;

public partial class MainWindowViewModel : BaseViewModel
{
    #region Properties

    public IAvaloniaReadOnlyList<IWorkspace> Workspaces { get; }
    
    [ObservableProperty] private IWorkspace _SelectedWorkspace;
    [ObservableProperty] private ObservableCollection<MenuItem> _MenuItems;

    #endregion

    #region Constructors
    
    public MainWindowViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager,
        ISukiDialogManager dialogManager,
        ISukiToastManager toastManager,
        IEnumerable<IWorkspace> workspaces) 
        : base(commandManager, eventManager, dialogManager, toastManager)
    {
        Workspaces = new AvaloniaList<IWorkspace>(workspaces
            .OrderBy(w => w.Index)
            .ThenBy(w => w.Title));

        SelectedWorkspace = Workspaces.First();

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
