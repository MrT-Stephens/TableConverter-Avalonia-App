using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;
using TableConverter.Utilities;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.ViewModels.Base;

public abstract partial class BaseDocumentViewModel : BaseViewModel, IPaneDocument, IIdentifiable, IDisposable
{
    #region Properties
    
    protected readonly IEventRegistrar _eventRegistrar = new EventRegistrar();
    
    public Guid ID { get; } = Guid.NewGuid();

    [ObservableProperty] private string _Title;
    [ObservableProperty] private bool _IsEnabled;

    public abstract bool CanClose { get; }
    
    public object Workspace { get; set; }

    #endregion

    #region Constructors

    protected BaseDocumentViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager,
        ISukiToastManager toastManager) 
        : base(commandManager, eventManager, dialogManager, toastManager)
    {
        Title = string.Empty;
        IsEnabled = true;
        Workspace = null!;
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

    public override void Dispose()
    {
        _eventRegistrar.Dispose();

        base.Dispose();
    }
    
    #endregion
}