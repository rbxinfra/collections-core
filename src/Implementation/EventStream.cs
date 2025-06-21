namespace Roblox.Collections;

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

using EventLog;

/// <summary>
/// Represents an event stream.
/// </summary>
/// <typeparam name="T">The type of the event.</typeparam>
public class EventStream<T>
{
    private readonly BlockingCollection<T> _Events;

    /// <summary>
    /// The name of type T.
    /// </summary>
    protected readonly string Type = typeof(T).Name;

    /// <summary>
    /// The <see cref="ILogger"/>
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// Event invoked when the queue is updated.
    /// </summary>
    public event Action<int> EventQueueUpdated;

    /// <summary>
    /// Event invoked when an event is added to the queue.
    /// </summary>
    public event Action EventAddedToQueue;

    /// <summary>
    /// Construct a new instance of <see cref="EventStream{T}"/>
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/></param>
    /// <param name="maximumEventsToStore">The maximum amount of events to store.</param>
    public EventStream(ILogger logger, int maximumEventsToStore)
    {
        Logger = logger;
        _Events = new BlockingCollection<T>(maximumEventsToStore);
    }

    /// <summary>
    /// Add an event to the stream.
    /// </summary>
    /// <param name="evt">The event.</param>
    public virtual void AddEvent(T evt)
    {
        if (!_Events.TryAdd(evt))
        {
            Logger.Error("{0}: Unable to add an event. Internal queue is full!", Type);

            return;
        }

        EventAddedToQueue?.Invoke();
        EventQueueUpdated?.Invoke(_Events.Count);
    }

    /// <summary>
    /// Get the specified number of events.
    /// </summary>
    /// <param name="count">The count.</param>
    /// <returns>The list of events.</returns>
    public IEnumerable<T> GetEvents(int count)
    {
        var events = new List<T>();
        int i = 0;
        while (i < count && _Events.TryTake(out var evt))
        {
            events.Add(evt);

            i++;
        }

        if (events.Count != 0) EventQueueUpdated?.Invoke(_Events.Count);

        return events;
    }

    /// <summary>
    /// Gets the total number of events.
    /// </summary>
    /// <returns>The total number of events.</returns>
    public int GetNumberOfEvents() => _Events.Count;
}
