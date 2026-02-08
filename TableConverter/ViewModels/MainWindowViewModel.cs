using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using Microsoft.Extensions.Options;
using SukiUI;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Configuration;
using TableConverter.Contracts.Events;
using TableConverter.Interfaces;
using TableConverter.Utilities.Extensions;
using TableConverter.Utilities.Interfaces;
using TableConverter.ViewModels.Base;

namespace TableConverter.ViewModels;

public partial class MainWindowViewModel : BaseViewModel
{
    #region Properties

    public IAvaloniaReadOnlyList<IWorkspace> Workspaces { get; }
    
    [ObservableProperty] private IWorkspace _SelectedWorkspace;
    [ObservableProperty] private ObservableCollection<MenuItem> _MenuItems;
    [ObservableProperty] private AppOptions _AppOptions;

    #endregion

    #region Constructors
    
    public MainWindowViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager,
        ISukiDialogManager dialogManager,
        ISukiToastManager toastManager,
        IEnumerable<IWorkspace> workspaces,
        IOptions<AppOptions> appOptions) 
        : base(commandManager, eventManager, dialogManager, toastManager)
    {
        MenuItems = [];
        AppOptions = appOptions.Value;
        
        Workspaces = new AvaloniaList<IWorkspace>(workspaces
            .OrderBy(w => w.Index)
            .ThenBy(w => w.Title));
        
        Workspaces.Cast<IInitialise>().ForEach(item => item.Initialise());

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
        
        // Ensure all directories exist
        AppOptions.BaseDocumentsPath.EnsureDirectoryExists();
        AppOptions.BaseConfigPath.EnsureDirectoryExists();
    }
    
    #endregion
}
