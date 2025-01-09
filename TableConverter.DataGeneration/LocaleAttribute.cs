namespace TableConverter.DataGeneration;

/// <summary>
///     Attribute to define the locale of a class.
///     Used to identify a dataset class as a locale.
/// </summary>
/// <param name="locale">The string on the locale. E.g. "en".</param>
[AttributeUsage(AttributeTargets.Class)]
public class LocaleAttribute(string locale) : Attribute
{
    public string Locale { get; private set; } = locale;
}