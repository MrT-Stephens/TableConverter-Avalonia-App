using System;

namespace TableConverter.Commands.Interfaces;

public interface ICommandManager : IDisposable
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
    /// The handler that will be invoked when the command is executed.
    /// </param>
    public void RegisterCommand(string name, ICommandHandlerBase handler);

    /// <summary>
    /// Registers a command with a name and a handler.
    /// </summary>
    /// <param name="name">
    /// The name of the command. This is used to retrieve the command later.
    /// </param>
    /// <param name="viewModel">
    /// An optional view model associated with the command. This can be used to provide context or state for the command.
    /// </param>
    public ICommandInstance RegisterCommandInstance(string name, object? viewModel = null);

    /// <summary>
    /// Retrieves a command by its name.
    /// </summary>
    /// <param name="name">
    /// The name of the command to retrieve. This should match the name used when registering the command.
    /// </param>
    /// <param name="viewModel">
    /// An optional view model associated with the command. This can be used to differentiate commands with the same name but different contexts.
    /// </param>
    /// <returns>
    /// The command associated with the specified name. If no command is found, the function will throw.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when no command with the specified name is registered.
    /// </exception>
    public ICommandInstance GetCommandInstance(string name, object? viewModel);
    
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
    public ICommandInstance this[string name] { get; }
    
    /// <summary>
    /// Retrieves a command by its name and associated view model.
    /// </summary>
    /// <param name="name">
    /// The name of the command to retrieve. This should match the name used when registering the command.
    /// </param>
    /// <param name="viewModel">
    /// An optional view model associated with the command. This can be used to differentiate commands with the same name but different contexts.
    /// </param>
    public ICommandInstance this[string name, object? viewModel] { get; }

    /// <summary>
    /// Raises <see cref="System.Windows.Input.ICommand.CanExecuteChanged"/> for every registered command
    /// instance so that bound controls re-query <c>CanExecute</c>. This is the equivalent of WPF's
    /// <c>CommandManager.InvalidateRequerySuggested()</c>.
    /// <para>
    /// Call this when state which <c>CanExecute</c> depends on changes but is not observable (for example a
    /// database refresh, a background service flag or a static/global setting).
    /// </para>
    /// </summary>
    public void InvalidateRequerySuggested();

    /// <summary>
    /// Raises <see cref="System.Windows.Input.ICommand.CanExecuteChanged"/> for every command instance whose
    /// parent is <paramref name="viewModel"/>.
    /// </summary>
    /// <param name="viewModel">
    /// The view model whose commands should be re-evaluated. Pass <c>null</c> to target the global
    /// (view model-less) command instances.
    /// </param>
    public void InvalidateRequerySuggested(object? viewModel);

    /// <summary>
    /// Raises <see cref="System.Windows.Input.ICommand.CanExecuteChanged"/> for a single command instance.
    /// </summary>
    /// <param name="name">
    /// The name of the command to re-evaluate.
    /// </param>
    /// <param name="viewModel">
    /// The view model the command instance is associated with.
    /// </param>
    public void InvalidateRequerySuggested(string name, object? viewModel);

    /// <summary>
    /// Releases every command instance created for <paramref name="viewModel"/> together with the event
    /// subscriptions those instances own (selection, view model and context notifications).
    /// <para>
    /// The command manager is a singleton, so without this the instances - and the view models they reference as
    /// their parent - are kept alive for the lifetime of the application. Call this from the view model's
    /// <c>Dispose</c>, otherwise a fresh instance is registered the next time the command is requested.
    /// </para>
    /// </summary>
    /// <param name="viewModel">
    /// The view model whose command instances should be released. Pass <c>null</c> to release the global
    /// (view model-less) command instances.
    /// </param>
    public void ReleaseCommandInstances(object? viewModel);
}