using System.Reflection;
using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration;

/// <summary>
/// Factory for creating locales.
/// Locales are classes that inherit from <see cref="LocaleBase"/> and are marked with the <see cref="LocaleAttribute"/>.
/// Locales store data for generating fake data.
/// </summary>
public static class LocaleFactory
{
    /// <summary>
    /// Create a locale based on the locale type.
    /// </summary>
    /// <param name="localeType">The locale type to create.</param>
    /// <returns>The created locale.</returns>
    /// <exception cref="Exception">Thrown when the locale type is not found.</exception>
    public static LocaleBase CreateLocale(string localeType = "en")
    {
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (!type.IsAssignableTo(typeof(LocaleBase)))
            {
                continue;
            }

            if (Attribute.GetCustomAttribute(type, typeof(LocaleAttribute)) is LocaleAttribute attribute &&
                attribute.Locale == localeType)
            {
                return Activator.CreateInstance(type) as LocaleBase ?? throw new Exception("Failed to create locale");
            }
        }

        throw new Exception("Locale not found");
    }

    /// <summary>
    /// Load all locale names from the assembly.
    /// </summary>
    /// <returns>An array of locale names.</returns>
    public static IReadOnlyList<string> LoadLocaleNames()
    {
        var locales = new List<string>();

        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (!type.IsAssignableTo(typeof(LocaleBase)))
            {
                continue;
            }

            if (Attribute.GetCustomAttribute(type, typeof(LocaleAttribute)) is LocaleAttribute attribute)
            {
                locales.Add(attribute.Locale);
            }
        }

        return locales;
    }
}