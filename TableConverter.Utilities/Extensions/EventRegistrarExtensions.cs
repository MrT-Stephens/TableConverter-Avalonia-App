using System.Collections.Specialized;
using System.ComponentModel;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities.Extensions;

public static class EventRegistrarExtensions
{
    public static IEventRegistrar RegisterPropertyChanged<T>(
        this IEventRegistrar eventRegistrar, T target, object? owner, PropertyChangedEventHandler handler) 
        where T : INotifyPropertyChanged
    {
        var handle = new PropertyChangedEventHandle(target, handler);
        eventRegistrar.Register(handle, owner);
        return eventRegistrar;
    }

    public static IEventRegistrar RegisterCollectionChanged<T>(
        this IEventRegistrar eventRegistrar, T target, object? owner, NotifyCollectionChangedEventHandler handler)
        where T : INotifyCollectionChanged
    {
        var handle = new CollectionChangedEventHandle(target, handler);
        eventRegistrar.Register(handle, owner);
        return eventRegistrar;
    }

    public static IEventRegistrar RegisterEvent<T>(
        this IEventRegistrar eventRegistrar, Action<T> add, Action<T> remove, object? owner, T handler)
        where T : Delegate
    {
        var handle = new EventHandle<T>(add, remove, handler);
        eventRegistrar.Register(handle, owner);
        return eventRegistrar;
    }
}

public sealed class PropertyChangedEventHandle(INotifyPropertyChanged target, PropertyChangedEventHandler handler) 
    : IEventHandle
{
    public void Add() => target.PropertyChanged += handler;
    public void Remove() => target.PropertyChanged -= handler;
}

public sealed class CollectionChangedEventHandle(INotifyCollectionChanged target, NotifyCollectionChangedEventHandler handler)
    : IEventHandle
{
    public void Add() => target.CollectionChanged += handler;
    public void Remove() => target.CollectionChanged -= handler;
}

public sealed class EventHandle<TDelegate>(Action<TDelegate> add, Action<TDelegate> remove, TDelegate handler)
    : IEventHandle where TDelegate : Delegate
{
    private readonly Action<TDelegate> _add = add ?? throw new ArgumentNullException(nameof(add));
    private readonly Action<TDelegate> _remove = remove ?? throw new ArgumentNullException(nameof(remove));
    private readonly TDelegate _handler = handler ?? throw new ArgumentNullException(nameof(handler));

    public void Add() => _add(_handler);
    public void Remove() => _remove(_handler);
}