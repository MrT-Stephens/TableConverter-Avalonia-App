using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;

namespace TableConverter.ViewModels.Base;

public abstract partial class BaseScopedPaneToolViewModel<TWorkspace> : BaseViewModel, IScopedPaneTool<TWorkspace>
{
    #region Properties

    [ObservableProperty] private string _Title;
    [ObservableProperty] private bool _IsEnabled;
    
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
    }

    #endregion

    #region Methods

    public override ICommandInstance this[string commandName] => _commandManager[commandName, Workspace];

    #endregion
}