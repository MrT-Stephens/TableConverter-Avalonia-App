namespace TableConverter.DataGeneration.Interfaces;

/// <summary>
///     Defines a builder for creating templated values with placeholders and customizable resolvers.
/// </summary>
/// <typeparam name="TDataset">The type of the dataset used for resolving values.</typeparam>
/// <typeparam name="TFaker">The type of the main faker instance.</typeparam>
public interface ITemplatedValueBuilder<TFaker, TDataset>
{
    /// <summary>
    ///     Sets the template string containing placeholders to be resolved.
    /// </summary>
    /// <param name="template">The template string.</param>
    /// <returns>The builder instance for method chaining.</returns>
    ITemplatedValueBuilder<TFaker, TDataset> SetTemplate(string template);

    /// <summary>
    ///     Adds a resolver function for a specific placeholder key.
    /// </summary>
    /// <param name="key">The placeholder key to resolve in the template.</param>
    /// <param name="resolver">A function to resolve the value for the placeholder.</param>
    /// <returns>The builder instance for method chaining.</returns>
    ITemplatedValueBuilder<TFaker, TDataset> AddPlaceholder(string key,
        Func<TFaker, TDataset, Randomizer, string> resolver);

    /// <summary>
    ///     Adds a random value resolver for a placeholder key, selecting from a list of possible values.
    /// </summary>
    /// <param name="key">The placeholder key to resolve in the template.</param>
    /// <param name="listResolver">A function to extract a list of values from the dataset.</param>
    /// <returns>The builder instance for method chaining.</returns>
    ITemplatedValueBuilder<TFaker, TDataset> AddRandomPlaceholder(string key,
        Func<TDataset, IReadOnlyList<string>> listResolver);

    /// <summary>
    ///     Builds the final templated value by resolving all placeholders with the dataset and randomizer.
    /// </summary>
    /// <param name="faker">The base faker instance for accessing other modules.</param>
    /// <param name="dataset">The dataset used for resolving placeholder values.</param>
    /// <param name="random">The randomizer for random value selection.</param>
    /// <returns>The resolved template string.</returns>
    string Build(TFaker faker, TDataset dataset, Randomizer random);
}