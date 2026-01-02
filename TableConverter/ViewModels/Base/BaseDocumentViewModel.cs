using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;
using TableConverter.Services;
using TableConverter.Utilities.Extensions;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.ViewModels.Base;

public abstract partial class BaseDocumentViewModel : BaseViewModel, IPaneDocument, ISerialisationData, IIdentifiable, IDisposable
{
    #region Properties
    
    protected readonly IEventRegistrar _eventRegistrar = new EventRegistrar();
    
    public Guid ID { get; } = Guid.NewGuid();

    [ObservableProperty] private string _Title;
    [ObservableProperty] private bool _IsEnabled;
    [ObservableProperty] private bool _IsDirty;

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
        IsDirty = false;
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

    public void ExportState(IDictionary<string, object> data)
    {
        data[nameof(Title)] = Title;
        data[nameof(IsEnabled)] = IsEnabled;
        data[nameof(IsDirty)] = IsDirty;
    }

    public void ImportState(IDictionary<string, object> data)
    {
        Title = data.GetOrThrow<string>(nameof(Title));
        IsEnabled = data.GetOrThrow<bool>(nameof(IsEnabled));
        IsDirty = data.GetOrThrow<bool>(nameof(IsDirty));
    }

    #endregion
    
    #region IDisposable

    public void Dispose()
    {
        _eventRegistrar.Dispose();
    }
    
    #endregion
}