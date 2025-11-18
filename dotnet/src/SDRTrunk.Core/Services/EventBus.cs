using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace SDRTrunk.Core.Services;

/// <summary>
/// Thread-safe event bus implementation for application-wide event communication.
/// Converts from the Java Google Guava EventBus pattern used in SDRTrunk.
/// </summary>
public class EventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _subscribers = new();
    private readonly ILogger<EventBus>? _logger;
    private readonly object _lock = new();

    public EventBus(ILogger<EventBus>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        var eventType = typeof(TEvent);

        lock (_lock)
        {
            if (!_subscribers.TryGetValue(eventType, out var handlers))
            {
                handlers = new List<Delegate>();
                _subscribers[eventType] = handlers;
            }

            if (!handlers.Contains(handler))
            {
                handlers.Add(handler);
                _logger?.LogDebug("Subscribed handler for event type {EventType}", eventType.Name);
            }
        }
    }

    /// <inheritdoc/>
    public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        var eventType = typeof(TEvent);

        lock (_lock)
        {
            if (_subscribers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);
                _logger?.LogDebug("Unsubscribed handler for event type {EventType}", eventType.Name);

                if (handlers.Count == 0)
                {
                    _subscribers.TryRemove(eventType, out _);
                }
            }
        }
    }

    /// <inheritdoc/>
    public void Publish<TEvent>(TEvent @event) where TEvent : class
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        var eventType = typeof(TEvent);
        List<Delegate> handlersCopy;

        lock (_lock)
        {
            if (!_subscribers.TryGetValue(eventType, out var handlers) || handlers.Count == 0)
            {
                _logger?.LogTrace("No subscribers for event type {EventType}", eventType.Name);
                return;
            }

            // Create a copy to avoid enumeration issues if handlers modify subscriptions
            handlersCopy = new List<Delegate>(handlers);
        }

        _logger?.LogDebug("Publishing event of type {EventType} to {Count} subscribers",
            eventType.Name, handlersCopy.Count);

        foreach (var handler in handlersCopy)
        {
            try
            {
                ((Action<TEvent>)handler)(@event);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error invoking event handler for {EventType}", eventType.Name);
            }
        }
    }
}
