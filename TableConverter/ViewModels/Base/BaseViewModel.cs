using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;

namespace TableConverter.ViewModels.Base;

public abstract partial class BaseViewModel : ObservableValidator
{
    #region Fields

    protected readonly ICommandManager _commandManager;
    protected readonly IEventManager _eventManager;
    protected readonly ISukiDialogManager _dialogManager;
    protected readonly ISukiToastManager _toastManager;

    #endregion

    #region Constructors

    public BaseViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager,
        ISukiDialogManager dialogManager,
        ISukiToastManager  toastManager)
    {
        _commandManager = commandManager;
        _eventManager = eventManager;
        _dialogManager = dialogManager;
        _toastManager = toastManager;

        _commandManager.OnCanExecute += OnCanExecuteCommand;
        _commandManager.OnExecute += OnExecuteCommand;
        _commandManager.OnExecuted += OnExecutedCommand;
        _commandManager.OnError += OnErrorCommand;
    }

    #endregion

    #region Command Handler Events

    protected virtual void OnCanExecuteCommand(object? sender, ICommandContext context)
    {
        // Override in derived classes if needed
    }

    protected virtual void OnExecuteCommand(object? sender, ICommandContext context)
    {
        // Override in derived classes if needed
    }
    
    protected virtual void OnExecutedCommand(object? sender, ICommandContext context)
    {
        // Override in derived classes if needed
    }
    
    protected virtual void OnErrorCommand(object? sender, Exception exception)
    {
        // Override in derived classes if needed
    }

    #endregion

    #region Misc

    public virtual ICommandInstance this[string commandName] => _commandManager[commandName, this];

    #endregion
}