using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities.Models;

public abstract class EventHandlerBase<TEventArgs> : IEventHandler<TEventArgs> where TEventArgs : EventArgs
{
    public Guid EventId { get; } = Guid.NewGuid();
    
    private readonly List<WeakDelegate> _handlers = [];
    private readonly Lock _handlersLock = new();
    
    public void Subscribe(EventHandler<TEventArgs> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        lock (_handlersLock)
        {
            _handlers.Add(new WeakDelegate(action));
        }
    }

    public void Unsubscribe(EventHandler<TEventArgs> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        lock (_handlersLock)
        {
            for (var i = _handlers.Count - 1; i >= 0; i--)
            {
                var handler = _handlers[i].Target;

                var isMatch = handler is EventHandler<TEventArgs> existing && existing.Equals(action);
                var isDead = handler is null;

                if (isMatch || isDead)
                {
                    _handlers[i].Clear();
                    _handlers.RemoveAt(i);
                }
            }
        }
    }

    public void Publish(TEventArgs args)
    {
        EventHandler<TEventArgs>[] snapshot;

        // Snapshot the live handlers under the lock, collecting the garbage in the same pass.
        lock (_handlersLock)
        {
            snapshot = new EventHandler<TEventArgs>[_handlers.Count];
            var count = 0;
            var writeIndex = 0;

            for (var i = 0; i < _handlers.Count; i++)
            {
                if (_handlers[i].Target is EventHandler<TEventArgs> handler)
                {
                    snapshot[count++] = handler;
                    _handlers[writeIndex++] = _handlers[i];
                    continue;
                }

                // The subscriber has been collected, so drop the weak reference.
                _handlers[i].Clear();
            }

            if (writeIndex < _handlers.Count)
            {
                _handlers.RemoveRange(writeIndex, _handlers.Count - writeIndex);
            }

            if (count != snapshot.Length)
            {
                Array.Resize(ref snapshot, count);
            }
        }

        // Invoke outside of the lock so handlers are free to (un)subscribe while being notified.
        foreach (var handler in snapshot)
        {
            handler(this, args);
        }
    }

    public void UnsubscribeAll(object subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        lock (_handlersLock)
        {
            for (var i = _handlers.Count - 1; i >= 0; i--)
            {
                var handler = _handlers[i].Target;

                // Also evict references whose subscriber has been garbage collected.
                var isSubscriberHandler = handler is EventHandler<TEventArgs> existing
                                          && ReferenceEquals(existing.Target, subscriber);
                var isDead = handler is null;

                if (isSubscriberHandler || isDead)
                {
                    _handlers[i].Clear();
                    _handlers.RemoveAt(i);
                }
            }
        }
    }
}