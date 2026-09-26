using System.Reflection;
using TableConverter.DataGeneration.Exceptions;
using TableConverter.DataGeneration.Interfaces;
using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration;

/// <summary>
///     Factory for creating locales.
///     Locales are classes that inherit from <see cref="LocaleBase" /> and are marked with the
///     <see cref="LocaleAttribute" />.
///     Locales store data for generating fake data.
/// </summary>
public static class LocaleFactory
{
    /// <summary>
    ///     The locale catalog is built once from the assembly's types and then reused. Reflecting over every type in
    ///     the assembly on each call is expensive, and the set of locales never changes at runtime.
    /// </summary>
    private static readonly Lazy<LocaleCatalog> Catalog =
        new(BuildCatalog, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    ///     Create a locale based on the locale type.
    /// </summary>
    /// <param name="localeType">The locale type to create.</param>
    /// <returns>The created locale.</returns>
    /// <exception cref="LocaleNotFoundException">
    ///     Thrown when no locale matching <paramref name="localeType" /> exists, or when the matching locale cannot be
    ///     instantiated.
    /// </exception>
    public static ILocale CreateLocale(string localeType = "en")
    {
        if (!Catalog.Value.TypesByName.TryGetValue(localeType, out var type))
        {
            throw new LocaleNotFoundException(localeType,
                $"No locale with the identifier '{localeType}' was found.");
        }

        return Activator.CreateInstance(type) as ILocale
               ?? throw new LocaleNotFoundException(localeType,
                   $"The locale '{localeType}' (type '{type.FullName}') could not be instantiated.");
    }

    /// <summary>
    ///     Load all locale names from the assembly.
    /// </summary>
    /// <returns>An array of locale names.</returns>
    public static IReadOnlyList<string> LoadLocaleNames()
    {
        return Catalog.Value.Names;
    }

    private static LocaleCatalog BuildCatalog()
    {
        var names = new List<string>();
        var typesByName = new Dictionary<string, Type>(StringComparer.Ordinal);

        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (!type.IsAssignableTo(typeof(ILocale))) continue;

            if (Attribute.GetCustomAttribute(type, typeof(LocaleAttribute)) is not LocaleAttribute attribute) continue;

            if (typesByName.TryAdd(attribute.Locale, type))
            {
                names.Add(attribute.Locale);
            }
        }

        return new LocaleCatalog(Array.AsReadOnly(names.ToArray()), typesByName);
    }

    private sealed record LocaleCatalog(IReadOnlyList<string> Names, IReadOnlyDictionary<string, Type> TypesByName);
}