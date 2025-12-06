using System.Diagnostics.CodeAnalysis;
using TableConverter.Interfaces;
using TableConverter.Utilities;

namespace TableConverter.Commands.Interfaces;

public interface ICommandContext : IHasSelectedItems
{
    /// <summary>
    /// Name of the command.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Indicates whether the command execution was cancelled.
    /// </summary>
    public bool Cancelled { get; set; }
    
    /// <summary>
    /// Reason for command cancellation, if any.
    /// </summary>
    public string CancelReason { get; set; }

    /// <summary>
    /// The parameter passed to the command, if any.
    /// </summary>
    public object? Parameter { get; set; }
    
    /// <summary>
    /// The view model associated with the command context, if any.
    /// </summary>
    public object? Parent { get; set; }

    /// <summary>
    /// The result of the command execution, if any.
    /// </summary>
    public Result<object>? Result { get; set; }

    /// <summary>
    /// Sets the result of the command execution to the provided object, wrapped in a successful Result.
    /// </summary>
    /// <param name="obj">
    /// The object to set as the result of the command execution.
    /// </param>
    /// <typeparam name="TObjectType">
    /// The type of the object to set as the result of the command execution.
    /// </typeparam>
    public void SetResult<TObjectType>(TObjectType obj);
    
    /// <summary>
    /// Sets the result of the command execution to an error state with the provided error message.
    /// </summary>
    /// <param name="errorMessage">
    /// The error message describing the reason for the failure.
    /// </param>
    public void SetErrorResult(string errorMessage);

    /// <summary>
    /// Attempts to get the result of the command execution as a specific type.
    /// </summary>
    /// <param name="result">
    /// The output parameter that will hold the result if it is of the specified type.
    /// </param>
    /// <typeparam name="TObjectType">
    /// The type of the result to retrieve from the command execution.
    /// </typeparam>
    /// <returns>
    /// True if the result is successfully retrieved and is of the specified type; otherwise, false.
    /// </returns>
    public bool TryGetResult<TObjectType>([NotNullWhen(true)] out Result<TObjectType>? result);
    
    /// <summary>
    /// Cancels the command execution with an optional reason.
    /// </summary>
    /// <param name="reason">
    /// The reason for cancelling the command execution.
    /// </param>
    public void Cancel(string reason = "");
}