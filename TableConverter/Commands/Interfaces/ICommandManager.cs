using System;
using System.Windows.Input;
using TableConverter.Interfaces;

namespace TableConverter.Commands.Interfaces;

public interface ICommandManager
{
    /// <summary>
    /// Raised when the command can execute state changes.
    /// </summary>
    public event EventHandler<ICommandContext> OnCanExecute;
    
    /// <summary>
    /// Raised when a command is about to be executed.
    /// </summary>
    public event EventHandler<ICommandContext> OnExecute;
    
    /// <summary>
    /// Raised when a command has been executed.
    /// </summary>
    public event EventHandler<ICommandContext> OnExecuted;
    
    /// <summary>
    /// Raised when an error occurs during command execution.
    /// </summary>
    public event EventHandler<Exception> OnError;

    /// <summary>
    /// Registers a command with a name and a handler.
    /// </summary>
    /// <param name="name">
    /// The name of the command. This is used to retrieve the command later.
    /// </param>
    /// <param name="handler">
    /// The handler that will be called when the command is executed.
    /// </param>
    public void RegisterCommand(string name, ICommandHandler handler);

    /// <summary>
    /// Registers a command with a name and an asynchronous handler.
    /// </summary>
    /// <param name="name">
    /// The name of the command. This is used to retrieve the command later.
    /// </param>
    /// <param name="handler">
    /// The asynchronous handler that will be called when the command is executed.
    /// </param>
    public void RegisterCommandAsync(string name, ICommandHandlerAsync handler);

    /// <summary>
    /// Retrieves a command by its name.
    /// </summary>
    /// <param name="name">
    /// The name of the command to retrieve. This should match the name used when registering the command.
    /// </param>
    /// <returns>
    /// The command associated with the specified name. If no command is found, the function will throw.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when no command with the specified name is registered.
    /// </exception>
    public ICommand GetCommand(string name);
    
    /// <summary>
    /// Indexer to retrieve a command by its name.
    /// </summary>
    /// <param name="name">
    /// The name of the command to retrieve. This should match the name used when registering the command.
    /// </param>
    /// <returns>
    /// The command associated with the specified name. If no command is found, it will throw an exception.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when no command with the specified name is registered.
    /// </exception>
    public ICommand this[string name] { get; }
}