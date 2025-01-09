using TableConverter.DataGeneration.Interfaces;

namespace TableConverter.DataGeneration.DataModels;

/// <summary>
///     A builder for creating templated values with placeholders and customizable resolvers.
/// </summary>
/// <typeparam name="TDataset">The type of the dataset used for resolving values.</typeparam>
/// <typeparam name="TFaker">The type of the main faker instance.</typeparam>
public class TemplatedValueBuilder<TFaker, TDataset> : ITemplatedValueBuilder<TFaker, TDataset>
{
    private readonly Dictionary<string, Func<TFaker, TDataset, Randomizer, string>> _placeholders = new();
    private string _template = "";

    /// <summary>
    ///     Sets the template string containing placeholders to be resolved.
    /// </summary>
    /// <param name="template">The template string with placeholders in the format {key}.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ITemplatedValueBuilder<TFaker, TDataset> SetTemplate(string template)
    {
        _template = template;
        return this;
    }

    /// <summary>
    ///     Adds a resolver function for a specific placeholder key.
    /// </summary>
    /// <param name="key">The placeholder key to resolve in the template.</param>
    /// <param name="resolver">A function to resolve the value for the placeholder.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ITemplatedValueBuilder<TFaker, TDataset> AddPlaceholder(string key,
        Func<TFaker, TDataset, Randomizer, string> resolver)
    {
        _placeholders[key] = resolver;
        return this;
    }

    /// <summary>
    ///     Adds a random value resolver for a placeholder key. The value is selected randomly from a list provided by the
    ///     dataset.
    /// </summary>
    /// <param name="key">The placeholder key to resolve in the template.</param>
    /// <param name="listResolver">A function that extracts a list of possible values from the dataset.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ITemplatedValueBuilder<TFaker, TDataset> AddRandomPlaceholder(string key,
        Func<TDataset, IReadOnlyList<string>> listResolver)
    {
        _placeholders[key] = (_, dataset, random) => random.GetRandomElement(listResolver(dataset));
        return this;
    }

    /// <summary>
    ///     Builds the final templated value by resolving all placeholders using the provided dataset and randomizer.
    /// </summary>
    /// <param name="faker">The faker instance for accessing modules.</param>
    /// <param name="dataset">The dataset to use for resolving placeholder values.</param>
    /// <param name="random">The randomizer used for random value selection.</param>
    /// <returns>The resolved template string.</returns>
    public string Build(TFaker faker, TDataset dataset, Randomizer random)
    {
        var result = _template;

        foreach (var placeholder in _placeholders)
        {
            var value = placeholder.Value(faker, dataset, random);
            result = result.Replace($"{{{placeholder.Key}}}", value);
        }

        return result;
    }
}