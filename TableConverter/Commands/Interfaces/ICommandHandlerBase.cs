namespace TableConverter.Commands.Interfaces;

public interface ICommandHandlerBase
{
    /// <summary>
    /// Represents the metadata for the command, including its name, description, and other relevant information.
    /// </summary>
    public ICommandMetadata CommandMetadata { get; }
    
    /// <summary>
    /// Checks if the command can be executed with the given parameter and context.
    /// </summary>
    /// <param name="parameter">
    /// The parameter to check if the command can be executed with. Can be null if the command does not require a parameter.
    /// </param>
    /// <param name="context">
    /// The context in which the command is checked, providing access to selected items and other relevant data.
    /// </param>
    /// <returns>
    /// True if the command can be executed with the given parameter and context; otherwise, false.
    /// </returns>
    public bool CanExecute(object? parameter, ICommandContext context);
}