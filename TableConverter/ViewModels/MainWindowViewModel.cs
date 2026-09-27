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
using TableConverter.Utilities;
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

    /// <summary>
    /// The window's subscriptions to the application's own events, kept so they can be released together.
    /// </summary>
    /// <remarks>
    /// The window is the one view model that lives for as long as the application does, so its
    /// subscriptions are never expected to be given back. They are still held here rather than subscribed
    /// straight onto the event: a subscription made through a registrar holds its handler, while the
    /// events themselves hold handlers weakly, so a handler reached only by its own event could be
    /// collected and the window would quietly stop navigating. Registering also names the window as the
    /// owner of what it subscribed, which is what a teardown would use.
    /// </remarks>
    private readonly IEventRegistrar _eventRegistrar = new EventRegistrar();

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

        _eventRegistrar.RegisterSubscription(
            _eventManager.GetEvent<PageNavigationRequestedEvent>(),
            (_, args) =>
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

    #region IDisposable

    public override void Dispose()
    {
        _eventRegistrar.Dispose();

        base.Dispose();
    }

    #endregion
}
