namespace SDRTrunk.Core.Services;

/// <summary>
/// Event bus for application-wide event communication.
/// Similar to the Java MyEventBus implementation.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Subscribe to events of a specific type.
    /// </summary>
    /// <typeparam name="TEvent">The event type to subscribe to</typeparam>
    /// <param name="handler">Handler to be called when event is published</param>
    void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;

    /// <summary>
    /// Unsubscribe from events of a specific type.
    /// </summary>
    /// <typeparam name="TEvent">The event type to unsubscribe from</typeparam>
    /// <param name="handler">Handler to remove</param>
    void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class;

    /// <summary>
    /// Publish an event to all subscribers.
    /// </summary>
    /// <typeparam name="TEvent">The event type</typeparam>
    /// <param name="event">The event to publish</param>
    void Publish<TEvent>(TEvent @event) where TEvent : class;
}
