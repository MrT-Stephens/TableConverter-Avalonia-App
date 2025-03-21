using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace TableConverter.DataGeneration.Exceptions;

/// <summary>
/// Exception for when a parameter is out of range.
/// </summary>
public class FakerArgumentOutOfRangeException : FakerArgumentException
{
    protected FakerArgumentOutOfRangeException(string? generatorName, string? paramName, object? actualValue,
        string message) : base(generatorName, paramName, actualValue, message)
    {
    }

    /// <summary>
    /// Creates an exception with the specified arguments.
    /// </summary>
    /// <param name="argument">
    /// The argument that caused the exception.
    /// </param>
    /// <param name="message">
    /// The message that describes the error.
    /// </param>
    /// <param name="paramName">
    /// The name of the parameter name that caused the exception.
    /// </param>
    /// <param name="generatorName">
    /// The name of the generator that caused the exception.
    /// </param>
    /// <exception cref="FakerArgumentException">
    /// Always throws an exception. The exception contains the specified arguments.
    /// </exception>
    [DoesNotReturn]
    public new static TReturn CreateException<TReturn>([NotNull] object? argument, string? message = null,
        [CallerArgumentExpression(nameof(argument))]
        string? paramName = null,
        [CallerMemberName] string? generatorName = null)
    {
        throw new FakerArgumentOutOfRangeException(generatorName, paramName, argument,
            message ?? "Argument out of range");
    }
}