namespace TableConverter.Commands.Interfaces;

public interface ICommandHandler : ICommandHandlerBase
{
    /// <summary>
    /// Executes the command with the given parameter and context.
    /// </summary>
    /// <param name="parameter">
    /// The parameter to execute the command with. Can be null if the command does not require a parameter.
    /// </param>
    /// <param name="context">
    /// The context in which the command is executed, providing access to selected items and other relevant data.
    /// </param>
    public void Execute(object? parameter, ICommandContext context);
}