using TableConverter.Utilities.Extensions;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities.Models;

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

    public void Publish(TEventArgs args)
    {
        foreach (var wd in _handlers)
        {
            if (wd.Target is EventHandler<TEventArgs> handler)
            {
                handler(this, args);
            }
        }
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