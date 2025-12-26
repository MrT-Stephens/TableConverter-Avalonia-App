using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;
using TableConverter.Services;

namespace TableConverter.ViewModels.Base;

public abstract partial class BaseDocumentViewModel : BaseViewModel, IPaneDocument, IDisposable
{
    #region Properties
    
    protected readonly IUndoRedo _undoRedo;
    protected readonly IEventRegistrar _eventRegistrar;
    
    public Guid ID { get; } = Guid.NewGuid();

    [ObservableProperty] private string _Title;
    [ObservableProperty] private bool _IsEnabled;
    [ObservableProperty] private bool _IsDirty;

    public abstract bool CanClose { get; }
    
    public object Workspace { get; set; }

    public abstract bool CanUndo { get; }
    
    public abstract bool CanRedo { get; }

    #endregion

    #region Constructors

    protected BaseDocumentViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager,
        ISukiToastManager toastManager,
        IUndoRedo undoRedo) 
        : base(commandManager, eventManager, dialogManager, toastManager)
    {
        Title = string.Empty;
        IsEnabled = true;
        IsDirty = false;
        Workspace = null!;
        
        _undoRedo = undoRedo;
        _eventRegistrar = new EventRegistrar();
    }

    #endregion

    #region Methods

    public virtual void OnActivate()
    {
        // Do nothing - Can be overriden
    }

    public void OnDeactivate()
    {
        // Do nothing - Can be overriden
    }

    #endregion
    
    #region IDisposable

    public void Dispose()
    {
        _eventRegistrar.Dispose();
    }
    
    #endregion
}