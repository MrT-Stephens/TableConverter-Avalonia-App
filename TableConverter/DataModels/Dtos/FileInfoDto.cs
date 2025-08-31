using System;
using System.Collections.Generic;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.DataModels.Dtos;

public record FileInfoDto(Guid Id, string Name, DateTime CreatedAt, string Path, bool IsGenerated = false)
    : IHasCache
{
    private readonly Dictionary<string, object> _Cache = new();
    
    public TValue GetFromCache<TValue>(string key, TValue defaultValue = default!)
    {
        if (_Cache.TryGetValue(key, out var value) && value is TValue typedValue)
            return typedValue;
        
        return defaultValue;
    }

    public void SetCacheValue<TValue>(string key, TValue value)
    {
        _Cache[key] = value!;
    }

    public void RemoveCacheValue(string key)
    {
        _Cache.Remove(key);
    }

    public void ClearCache()
    {
        _Cache.Clear();
    }
}