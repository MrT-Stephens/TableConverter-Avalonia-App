using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;

namespace TableConverter.ViewModels;

public class BaseViewModel : ObservableValidator
{
    #region Fields

    protected readonly ICommandManager _commandManager;
    protected readonly IEventManager _eventManager;

    #endregion

    #region Constructors

    public BaseViewModel(ICommandManager commandManager, IEventManager eventManager)
    {
        _commandManager = commandManager;
        _eventManager = eventManager;

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

    public ICommand this[string commandName] => _commandManager[commandName];

    #endregion
}