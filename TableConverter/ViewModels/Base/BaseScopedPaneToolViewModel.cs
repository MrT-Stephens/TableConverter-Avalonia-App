using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Contracts.Events;
using TableConverter.Interfaces;
using TableConverter.Utilities;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.ViewModels.Base;

public abstract partial class BaseScopedPaneToolViewModel<TWorkspace> : BaseViewModel, IScopedPaneTool<TWorkspace>, IDisposable
{
    #region Properties

    protected readonly IEventRegistrar _eventRegistrar = new EventRegistrar();
    
    [ObservableProperty] private string _Title;
    [ObservableProperty] private bool _IsEnabled;
    
    public TWorkspace Workspace { get; set; }
    
    object IPane.Workspace
    {
        get => Workspace!;
        set => Workspace = (TWorkspace)value;
    }

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
        Workspace = default!;
    }

    #endregion

    #region Methods

    public override void Initialise()
    {
        base.Initialise();
        
        _eventManager.GetEvent<WorkspaceDocumentSelectedEvent>()
            .Subscribe((_, args) =>
            {
                OnSelectedDocumentChanged(args.Workspace, args.OldDocument, args.NewDocument);
            });
    }

    public override ICommandInstance this[string commandName] => _commandManager[commandName, Workspace];

    public virtual void OnActivate()
    {
        // Do nothing - Can be overriden
    }

    public void OnDeactivate()
    {
        // Do nothing - Can be overriden
    }

    protected virtual void OnSelectedDocumentChanged(IWorkspace workspace, IPaneDocument? oldDocument,
        IPaneDocument? newDocument)
    {
        // Do nothing - Can be overriden
    }
    
    public void Dispose()
    {
        _eventRegistrar.Dispose();
    }

    #endregion
}