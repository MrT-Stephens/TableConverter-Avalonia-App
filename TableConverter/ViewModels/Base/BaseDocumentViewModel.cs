using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;

namespace TableConverter.ViewModels.Base;

public abstract partial class BaseDocumentViewModel : BaseViewModel, IPaneDocument
{
    #region Properties

    [ObservableProperty] private string _Title;
    [ObservableProperty] private bool _IsEnabled;
    [ObservableProperty] private bool _IsDirty;

    public abstract bool CanClose { get; }

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
}