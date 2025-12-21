using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Avalonia.Collections;
using TableConverter.Extensions;
using TableConverter.Interfaces;
using TableConverter.Services;
using TableConverter.Utilities.Extensions;

namespace TableConverter.Common;

public class ItemChangedEventArgs(object? item) : EventArgs
{
    public object? Item { get; } = item;
}

public class SelectedItemsCollection : IList, INotifyCollectionChanged, INotifyPropertyChanged, IDisposable
{
    #region Fields

    private readonly EventRegistrar _eventRegistrar = new();
    
    private readonly List<object> _flatList = [];
    private readonly Dictionary<Type, List<object>> _typeCache = [];

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<ItemChangedEventArgs>? ItemChanged; 
    
    #endregion

    #region Add / Remove / Clear

    public void Add(object? item)
    {
        if (item == null) return;

        if (_flatList.Contains(item)) return;

        _flatList.Add(item);

        var type = item.GetType();
        
        if (!_typeCache.TryGetValue(type, out var list))
        {
            list = [];
            _typeCache[type] = list;
        }
        
        list.Add(item);

        AttachItemEvents(item, type);

        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
    }

    public void Remove(object? item)
    {
        if (item == null) return;

        if (!_flatList.Remove(item)) return;

        var type = item.GetType();

        if (_typeCache.TryGetValue(type, out var list))
        {
            list.Remove(item);
            if (list.Count == 0)
                _typeCache.Remove(type);
        }
        
        _eventRegistrar.Clear(item);

        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
        
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
    }

    public void Clear()
    {
        _eventRegistrar.ClearAll();
        _flatList.Clear();
        _typeCache.Clear();
        RaiseReset();
    }

    #endregion

    #region Typed Retrieval

    public IReadOnlyCollection<T> Get<T>()
    {
        if (_typeCache.TryGetValue(typeof(T), out var list))
            return list.Cast<T>().ToList().AsReadOnly();
        
        return [];
    }

    public T? GetSingle<T>()
    {
        if (_typeCache.TryGetValue(typeof(T), out var list) && list.Count > 0)
            return (T)list[0];
        
        return default;
    }

    public object? GetSingle(Type type)
    {
        if (_typeCache.TryGetValue(type, out var list) && list.Count > 0)
            return list[0];
        
        return null;
    }
    
    public void RemoveAll<T>()
    {
        RemoveAll(typeof(T));
    }
    
    public void RemoveAll(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (!_typeCache.TryGetValue(type, out var list) || list.Count == 0)
            return;
        
        list.ForEach(Remove);
    }

    #endregion

    #region IList Implementation

    public int Count => _flatList.Count;
    public bool IsReadOnly => false;
    public bool IsFixedSize => false;
    public bool IsSynchronized => false;
    public object SyncRoot => this;

    int IList.Add(object? value)
    {
        Add(value);
        return Count - 1;
    }

    bool IList.Contains(object? value) => 
        value != null && _flatList.Contains(value);

    int IList.IndexOf(object? value) => 
        value != null ? _flatList.IndexOf(value) : -1;

    void IList.Insert(int index, object? value)
    {
        if (value == null) return;
        
        if (_flatList.Contains(value)) return;

        _flatList.Insert(index, value);

        var type = value.GetType();
        
        if (!_typeCache.TryGetValue(type, out var list))
        {
            list = [];
            _typeCache[type] = list;
        }
        
        list.Add(value);
        
        AttachItemEvents(value, type);

        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, index));
        
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
    }

    void IList.RemoveAt(int index)
    {
        if (index < 0 || index >= _flatList.Count) return;

        var item = _flatList[index];
        Remove(item);
    }

    public void CopyTo(Array array, int index) => 
        _flatList.ToArray().CopyTo(array, index);

    public IEnumerator GetEnumerator() => 
        _flatList.GetEnumerator();

    public object? this[int index]
    {
        get => _flatList[index];
        set
        {
            if (value == null) return;

            if (index < 0 || index >= _flatList.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var oldItem = _flatList[index];
            Remove(oldItem);

            _flatList.Insert(index, value);

            var type = value.GetType();
            
            if (!_typeCache.TryGetValue(type, out var list))
            {
                list = [];
                _typeCache[type] = list;
            }

            AttachItemEvents(value, type);
            
            list.Add(value);

            RaiseReset();
        }
    }

    #endregion

    #region IDisposable Implementation

    public void Dispose()
    {
        Clear();
    }

    #endregion

    #region Misc Methods

    private void AttachItemEvents(object item, Type type)
    {
        if (item is not INotifyPropertyChanged basePropertyChanged) 
            return;

        string[] ignoreProperties =
        [
            nameof(IHasSelectedItems.SelectedItems)
        ];
        
        _eventRegistrar.RegisterPropertyChanged(basePropertyChanged, item, OnItemPropertyChanged);
            
        foreach (var info in item.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetIndexParameters().Length == 0 || !ignoreProperties.Contains(p.Name)))
        {
            object? value;

            try
            {
                value = info.GetValue(item);
            }
            catch
            {
                continue;
            }

            switch (value)
            {
                case INotifyPropertyChanged npc:
                    _eventRegistrar.RegisterPropertyChanged(npc, item, OnItemPropertyChanged);
                    break;

                case INotifyCollectionChanged ncc:
                    _eventRegistrar.RegisterCollectionChanged(ncc, item, OnItemPropertyChanged);
                    break;
            }
        }
    }
    
    private void RaiseReset()
    {
        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
    }

    private void OnItemPropertyChanged(object? sender, object? _)
    {
        ItemChanged?.Invoke(this, new ItemChangedEventArgs(sender));
    }
    
    #endregion
}
