using System.Threading.Tasks;

namespace TableConverter.Commands.Interfaces;

public interface ICommandHandlerAsync : ICommandHandlerBase
{
    /// <summary>
    /// Executes the command with the given parameter and context asynchronously.
    /// </summary>
    /// <param name="parameter">
    /// The parameter to execute the command with. Can be null if the command does not require a parameter.
    /// </param>
    /// <param name="context">
    /// The context in which the command is executed, providing access to selected items and other relevant data.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task completes when the command execution is finished.
    /// </returns>
    public Task Execute(object? parameter, ICommandContext context);
}