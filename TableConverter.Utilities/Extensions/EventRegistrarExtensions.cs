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

    /// <summary>
    ///     Subscribes <paramref name="handler" /> to <paramref name="eventHandler" /> and tracks the subscription so it is
    ///     removed when the registrar is disposed. Use this instead of calling <c>Subscribe</c> directly, otherwise the
    ///     subscription is never released.
    /// </summary>
    /// <returns>
    ///     The registration that owns the subscription. Disposing it removes just this subscription; it is also removed
    ///     when the registrar is disposed.
    /// </returns>
    public static IEventRegistration RegisterSubscription<TEventArgs>(
        this IEventRegistrar eventRegistrar,
        IEventHandler<TEventArgs> eventHandler,
        EventHandler<TEventArgs> handler,
        object? owner = null)
        where TEventArgs : EventArgs
    {
        var subscription = new EventHandlerSubscriptionHandle<TEventArgs>(eventHandler, handler);
        return eventRegistrar.Register(subscription, owner);
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

public sealed class EventHandlerSubscriptionHandle<TEventArgs>(
    IEventHandler<TEventArgs> eventHandler,
    EventHandler<TEventArgs> handler)
    : IEventHandle where TEventArgs : EventArgs
{
    private readonly IEventHandler<TEventArgs> _eventHandler =
        eventHandler ?? throw new ArgumentNullException(nameof(eventHandler));

    private readonly EventHandler<TEventArgs> _handler = handler ?? throw new ArgumentNullException(nameof(handler));

    public void Add() => _eventHandler.Subscribe(_handler);

    public void Remove() => _eventHandler.Unsubscribe(_handler);
}

