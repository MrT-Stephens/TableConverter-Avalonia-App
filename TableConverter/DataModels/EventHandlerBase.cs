using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TableConverter.Interfaces;
using TableConverter.Utilities;
using TableConverter.Utilities.Extensions;

namespace TableConverter.DataModels;

public abstract class EventHandlerBase<TEventArgs> : IEventHandler<TEventArgs> where TEventArgs : EventArgs
{
    public Guid EventId { get; } = Guid.NewGuid();
    
    private readonly List<WeakDelegate> _Handlers = [];
    private readonly Lock _HandlersLock = new();
    
    public void Subscribe(EventHandler<TEventArgs> action)
    {
        lock (_HandlersLock)
        {
            _Handlers.Add(new WeakDelegate(action));
        }
    }

    public void Unsubscribe(EventHandler<TEventArgs> action)
    {
        lock (_HandlersLock)
        {
            UnsubscribeWithoutLock(action);
        }
    }

    public void Publish(object? sender, TEventArgs args)
    {
        _Handlers
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
        lock (_HandlersLock)
        {
            _Handlers
                .Where(wd => wd.Target is null 
                    || (wd.Target is EventHandler<TEventArgs> handler && handler.Target == subscriber))
                .ForEach(wd =>
                {
                    if (wd.Target is not null)
                    {
                        _Handlers.Remove(wd);
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
        _Handlers
            .Where(wd => wd.Target is EventHandler<TEventArgs> handler && handler.Equals(action))
            .ForEach(wd =>
            {
                wd.Clear();
                _Handlers.Remove(wd);
            });
    }
}