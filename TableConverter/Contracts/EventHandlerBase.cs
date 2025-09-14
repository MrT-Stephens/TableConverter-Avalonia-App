using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TableConverter.Interfaces;
using TableConverter.Utilities;
using TableConverter.Utilities.Extensions;

namespace TableConverter.Contracts;

public abstract class EventHandlerBase<TEventArgs> : IEventHandler<TEventArgs> where TEventArgs : EventArgs
{
    public Guid EventId { get; } = Guid.NewGuid();
    
    private readonly List<WeakDelegate> _handlers = [];
    private readonly Lock _handlersLock = new();
    
    public void Subscribe(EventHandler<TEventArgs> action)
    {
        lock (_handlersLock)
        {
            _handlers.Add(new WeakDelegate(action));
        }
    }

    public void Unsubscribe(EventHandler<TEventArgs> action)
    {
        lock (_handlersLock)
        {
            UnsubscribeWithoutLock(action);
        }
    }

    public void Publish(object? sender, TEventArgs args)
    {
        _handlers
            .ForEach(wd =>
            {
                if (wd.Target is EventHandler<TEventArgs> handler)
                {
                    handler(sender, args);
                }
            });
    }
    
    public void UnsubscribeAll(object subscriber)
    {
        lock (_handlersLock)
        {
            _handlers
                .Where(wd => wd.Target is null 
                    || (wd.Target is EventHandler<TEventArgs> handler && handler.Target == subscriber))
                .ForEach(wd =>
                {
                    if (wd.Target is not null)
                    {
                        _handlers.Remove(wd);
                    }
                    else
                    {  
                        UnsubscribeWithoutLock((EventHandler<TEventArgs>)wd.Target!);
                    }
                });
        }
    }

    private void UnsubscribeWithoutLock(EventHandler<TEventArgs> action)
    {
        _handlers
            .Where(wd => wd.Target is EventHandler<TEventArgs> handler && handler.Equals(action))
            .ForEach(wd =>
            {
                wd.Clear();
                _handlers.Remove(wd);
            });
    }
}