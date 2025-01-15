namespace TableConverter.DataGeneration.Interfaces;

/// <summary>
///     Represents a locale, typically used to define a language or region-specific set of data.
/// </summary>
public interface ILocale
{
    /// <summary>
    ///     Gets the title or name of the locale, such as the language or region it represents (e.g., "English", "French",
    ///     "en-US").
    /// </summary>
    public string Title { get; }

    /// <summary>
    ///     Gets the code associated with the locale, typically a language code or regional code (e.g., "en", "fr", "en-US").
    /// </summary>
    public string Code { get; }
}