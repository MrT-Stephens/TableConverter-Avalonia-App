using System.Windows.Input;

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
    /// The top-level command handler which is used to construct the command
    /// </summary>
    public ICommandHandlerBase Handler { get; }
    
    /// <summary>
    /// The context of the executing command
    /// </summary>
    public ICommandContext Context { get; }
}