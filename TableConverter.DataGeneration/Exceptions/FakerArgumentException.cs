using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace TableConverter.DataGeneration.Exceptions;

/// <summary>
/// The exception that is thrown when one of the arguments provided to a method is not valid.
/// </summary>
public class FakerArgumentException : Exception
{
    protected FakerArgumentException(string? generatorName, string? paramName, object? actualValue, string message) :
        base(message)
    {
        ParamName = paramName;
        GeneratorName = generatorName;
        ActualValue = actualValue;
    }

    public string? ParamName { get; }

    public string? GeneratorName { get; }

    public object? ActualValue { get; }

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
    public static TReturn CreateException<TReturn>([NotNull] object? argument, string? message = null,
        [CallerArgumentExpression(nameof(argument))]
        string? paramName = null,
        [CallerMemberName] string? generatorName = null)
    {
        throw new FakerArgumentException(generatorName, paramName, argument, message ?? "Invalid argument");
    }

    public override string ToString()
    {
        return
            $"An exception occurred in the generator '{GeneratorName}' with the parameter '{ParamName}' and the value '{ActualValue}'. Inner message: '{Message}'.";
    }
}