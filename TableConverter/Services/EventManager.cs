using System;
using System.Collections.Concurrent;
using TableConverter.Interfaces;
using TableConverter.Utilities.Extensions;

namespace TableConverter.Services;

public class EventManager : IEventManager
{
    private readonly ConcurrentDictionary<Type, IEventHandler> _eventHandlers = new();

    public TEventType GetEvent<TEventType>() where TEventType : IEventHandler
    {
        return (TEventType)_eventHandlers.GetOrAdd(typeof(TEventType), CreateEventInstance);
    }

    public void UnregisterAllEvents(object subscriber)
    {
        _eventHandlers.Values
            .ForEach(handler => handler.UnsubscribeAll(subscriber));
    }

    private static IEventHandler CreateEventInstance(Type eventType)
    {
#if DEBUG
        if (!typeof(IEventHandler).IsAssignableFrom(eventType))
            throw new InvalidOperationException($"The type {eventType.FullName} does not implement {nameof(IEventHandler)}.");
        
        if (eventType is not { IsClass: true, IsInterface: false })
            throw new InvalidOperationException($"The type {eventType.FullName} is not a class or is an interface.");
#endif
        
        return Activator.CreateInstance(eventType) as IEventHandler
            ?? throw new InvalidOperationException($"Could not create an instance of type {eventType.FullName}.");
    }
}