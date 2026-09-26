using System.Collections.Generic;
using TableConverter.Contracts;

namespace TableConverter.Interfaces;

public interface IDataGenerationTypes
{
    /// <summary>
    /// Gets the list of available data generation types.
    /// </summary>
    public IReadOnlyList<DataGenerationType> Types { get; }

    /// <summary>
    /// Gets the list of available locales for data generation.
    /// </summary>
    public IReadOnlyList<string> AvailableLocales { get; }

    /// <summary>
    /// Sets the locale for data generation.
    /// </summary>
    /// <param name="locale">
    /// The locale to set for data generation.
    /// </param>
    public void SetLocale(string locale);

    /// <summary>
    /// Sets the seed for data generation. A seed allows for reproducible data generation.
    /// </summary>
    /// <param name="seed">
    /// The seed value to use for data generation.
    /// </param>
    public void SetSeed(int seed);
}