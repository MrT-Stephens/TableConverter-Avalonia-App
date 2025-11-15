using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Contracts.Events;
using TableConverter.Interfaces;

namespace TableConverter.ViewModels.Base;

public abstract partial class BaseScopedPaneToolViewModel<TWorkspace> : BaseViewModel, IScopedPaneTool<TWorkspace>
{
    #region Properties

    [ObservableProperty] private string _Title;
    [ObservableProperty] private bool _IsEnabled;
    [ObservableProperty] private IPaneDocument? _SelectedDocument;
    
    public TWorkspace Workspace { get; set; }
    
    object IPaneTool.Workspace
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
        
        _eventManager.GetEvent<WorkspaceDocumentSelectedEvent>()
            .Subscribe((_, args) => OnSelectedDocumentChanged(args.Workspace, args.OldDocument, args.NewDocument));
    }

    #endregion

    #region Methods

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
        if ((IWorkspace)Workspace! == workspace)
        {
            SelectedDocument = newDocument;
        }
    }

    #endregion
}