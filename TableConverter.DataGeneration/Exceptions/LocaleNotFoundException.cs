namespace TableConverter.DataGeneration.Exceptions;

/// <summary>
///     The exception that is thrown when a locale identifier cannot be resolved to a usable locale.
/// </summary>
public sealed class LocaleNotFoundException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LocaleNotFoundException" /> class.
    /// </summary>
    /// <param name="localeType">The locale identifier that could not be resolved (e.g. "en").</param>
    /// <param name="message">The message that describes the error.</param>
    public LocaleNotFoundException(string localeType, string message) : base(message)
    {
        LocaleType = localeType;
    }

    /// <summary>
    ///     Gets the locale identifier that could not be resolved.
    /// </summary>
    public string LocaleType { get; }

    public override string ToString()
    {
        return $"An exception occurred while resolving the locale '{LocaleType}'. Inner message: '{Message}'.";
    }
}

