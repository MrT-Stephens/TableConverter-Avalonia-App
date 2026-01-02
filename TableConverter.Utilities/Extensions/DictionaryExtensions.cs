namespace TableConverter.Utilities.Extensions;

public static class DictionaryExtensions
{
    /// <summary>
    /// Removes a range of keys from the dictionary.
    /// </summary>
    /// <param name="dictionary">The dictionary from which items will be removed.</param>
    /// <param name="keys">The keys to remove from the dictionary.</param>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    public static void RemoveRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys)
    {
        foreach (var key in keys)
        {
            dictionary.Remove(key);
        }
    }
    
    /// <summary>
    /// Gets the value associated with the specified key.
    /// If the key does not exist, adds a new value created by the valueFactory.
    /// </summary>
    /// <param name="dictionary">
    /// The dictionary to operate on.
    /// </param>
    /// <param name="key">
    /// The key whose value to get or add.
    /// </param>
    /// <param name="valueFactory">
    /// The function to create a new value if the key does not exist.
    /// </param>
    /// <typeparam name="TKey">
    /// The type of keys in the dictionary.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The type of values in the dictionary.
    /// </typeparam>
    /// <returns>
    /// The value associated with the specified key.
    /// </returns>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFactory)
    {
        if (!dictionary.TryGetValue(key, out var value))
        {
            value = valueFactory();
            dictionary[key] = value;
        }

        return value;
    }
    
    /// <summary>
    /// Gets the value associated with the specified key or throws KeyNotFoundException if the key does not exist.
    /// </summary>
    /// <param name="dictionary">
    /// The dictionary to operate on.
    /// </param>
    /// <param name="key">
    /// The key whose value to get.
    /// </param>
    /// <typeparam name="TKey">
    /// The type of keys in the dictionary.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The type of values in the dictionary.
    /// </typeparam>
    /// <returns>
    /// The value associated with the specified key.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when the key does not exist in the dictionary.
    /// </exception>
    public static TValue GetOrThrow<TValue>(this IDictionary<string, object> dictionary, string key)
    {
        if (!dictionary.TryGetValue(key, out var value)
            || value is not TValue typedValue)
        {
            throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");
        }

        return typedValue;
    }
}