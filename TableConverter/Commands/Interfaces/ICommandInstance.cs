using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace TableConverter.Commands.Interfaces;

public interface ICommandInstance
{
    /// <summary>
    /// The metadata of the command
    /// </summary>
    public ICommandMetadata Metadata { get; }
    
    /// <summary>
    /// The bindable command
    /// </summary>
    public ICommand Command { get; }
    
    /// <summary>
    /// The bindable command exposed as a relay command so callers can force a re-evaluation of
    /// <see cref="ICommand.CanExecute(object)"/> without a cast.
    /// </summary>
    public IRelayCommand RelayCommand { get; }
    
    /// <summary>
    /// The top-level command handler which is used to construct the command
    /// </summary>
    public ICommandHandlerBase Handler { get; }
    
    /// <summary>
    /// The context of the executing command
    /// </summary>
    public ICommandContext Context { get; }

    /// <summary>
    /// Forces bound controls to re-query <see cref="ICommand.CanExecute(object)"/>.
    /// This is the per-command equivalent of WPF's <c>CommandManager.InvalidateRequerySuggested()</c>.
    /// </summary>
    public void RaiseCanExecuteChanged();
}