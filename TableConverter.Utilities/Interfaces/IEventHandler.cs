namespace TableConverter.Utilities.Interfaces;

public interface IEventHandler
{
    /// <summary>
    /// A unique identifier for the event handler instance.
    /// </summary>
    public Guid EventId { get; }
    
    /// <summary>
    /// Unsubscribes all event handlers associated with the specified subscriber object.
    /// </summary>
    /// <param name="subscriber">
    /// The subscriber object whose event handlers should be unsubscribed.
    /// </param>
    public void UnsubscribeAll(object subscriber);
}

public interface IEventHandler<TEventArgs> : IEventHandler where TEventArgs : EventArgs
{
    /// <summary>
    /// Subscribes to the event with the specified action.
    /// </summary>
    /// <param name="action">
    /// The action to be invoked when the event is published.
    /// </param>
    public void Subscribe(EventHandler<TEventArgs> action);
    
    /// <summary>
    /// Unsubscribes from the event with the specified action.
    /// </summary>
    /// <param name="action">
    /// The action to be removed from the event subscribers.
    /// </param>
    public void Unsubscribe(EventHandler<TEventArgs> action);

    /// <summary>
    /// Publishes the event to all subscribed actions.
    /// </summary>
    /// <param name="sender">
    /// The sender of the event.
    /// </param>
    /// <param name="args">
    /// The event data.
    /// </param>
    public void Publish(TEventArgs args);
}