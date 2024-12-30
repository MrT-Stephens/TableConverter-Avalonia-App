using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.Interfaces;

/// <summary>
///     Interface representing a Faker instance for generating localized random data.
/// </summary>
public interface IFaker
{
    /// <summary>
    ///     Gets or sets the locale information used for generating data.
    ///     This property determines the localized data set (e.g., language, culture) used during generation.
    /// </summary>
    public LocaleBase Locale { get; protected set; }

    /// <summary>
    ///     Gets or sets the randomizer instance used to produce random values.
    ///     The randomizer provides methods for generating random numbers, strings, and other values.
    /// </summary>
    public Randomizer Randomizer { get; protected set; }

    /// <summary>
    ///     Seeds the randomizer with a specific value to ensure reproducibility of generated data.
    ///     By setting a seed, the same sequence of random values can be produced consistently.
    /// </summary>
    /// <param name="seed">The integer value to seed the randomizer.</param>
    public void Seed(int seed);
}