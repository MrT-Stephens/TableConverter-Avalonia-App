namespace TableConverter.Utilities.Interfaces;

public interface IEventManager
{
    /// <summary>
    /// Gets the event handler instance of the specified type.
    /// </summary>
    /// <typeparam name="TEventType">
    /// The type of the event handler to retrieve. Must implement IEventHandler.
    /// </typeparam>
    /// <returns>
    /// The instance of the specified event handler type.
    /// </returns>
    public TEventType GetEvent<TEventType>() where TEventType : IEventHandler;
    
    /// <summary>
    /// Unregisters all event handlers associated with the specified subscriber object.
    /// </summary>
    /// <param name="subscriber">
    /// The subscriber object whose event handlers should be unregistered.
    /// </param>
    public void UnregisterAllEvents(object subscriber);
}