namespace Roblox.Collections;

using System;

using EventLog;

/// <summary>
/// An <see cref="EventStream{T}"/> with debouncing.
/// </summary>
/// <typeparam name="TEvent">The type of the event.</typeparam>
/// <typeparam name="TKey">The type of the key.</typeparam>
public class DebouncingEventStream<TEvent, TKey> : EventStream<TEvent>, IDisposable
{
    private readonly ExpirableDictionary<TKey, string> _Debouncer;

    /// <summary>
    /// Construct a new instance of <see cref="DebouncingEventStream{TEvent, TKey}"/>
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/></param>
    /// <param name="maximumEventsToStore">The maximum amount of events to store.</param>
    /// <param name="debunceWindow">The debounce window.</param>
    public DebouncingEventStream(ILogger logger, int maximumEventsToStore, TimeSpan debunceWindow)
        : base(logger, maximumEventsToStore)
    {
        _Debouncer = new ExpirableDictionary<TKey, string>(debunceWindow);

        _Debouncer.ExceptionOccurred += ex => Logger.Error("Error in DebouncingEventStream: {0}", ex);
    }

    /// <summary>
    /// Should the stream debounce for the specified key
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>True if the key should debounce.</returns>
    public bool ShouldDebounce(TKey key) => _Debouncer.ContainsKey(key);

    /// <summary>
    /// Add the specified key to the debouncer.
    /// </summary>
    /// <param name="key"></param>
    public void AddToDebouncer(TKey key) => _Debouncer.Set(key, string.Empty);

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose() => _Debouncer.Dispose();
}
