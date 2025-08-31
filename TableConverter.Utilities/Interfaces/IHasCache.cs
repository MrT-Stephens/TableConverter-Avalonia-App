namespace TableConverter.Utilities.Interfaces;

/// <summary>
/// Defines a contract for classes that support caching functionality.
/// </summary>
public interface IHasCache
{
    /// <summary>
    /// Gets a cached value associated with the specified key.
    /// If the key does not exist or the value cannot be cast to the specified type, returns the default value.
    /// </summary>
    /// <param name="key">
    /// The key associated with the cached value.
    /// </param>
    /// <param name="defaultValue">
    /// The default value to return if the key does not exist or the value cannot be cast to the specified type.
    /// </param>
    /// <typeparam name="TValue">
    /// The type of the cached value.
    /// </typeparam>
    /// <returns>
    /// The cached value associated with the specified key, or the default value if the key does not exist or the value cannot be cast to the specified type.
    /// </returns>
    public TValue GetFromCache<TValue>(string key, TValue defaultValue = default!);
    
    /// <summary>
    /// Sets a cached value associated with the specified key.
    /// </summary>
    /// <param name="key">
    /// The key associated with the cached value.
    /// </param>
    /// <param name="value">
    /// The value to be cached.
    /// </param>
    /// <typeparam name="TValue">
    /// The type of the cached value.
    /// </typeparam>
    public void SetCacheValue<TValue>(string key, TValue value);
    
    /// <summary>
    /// Removes a cached value associated with the specified key.
    /// </summary>
    /// <param name="key">
    /// The key associated with the cached value to be removed.
    /// </param>
    public void RemoveCacheValue(string key);
    
    /// <summary>
    /// Clears all cached values.
    /// </summary>
    public void ClearCache();
}