namespace Roblox.Collections;

using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Interfaces;

/// <summary>
/// Represents a deduplicating buffered queue.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of the value.</typeparam>
/// <seealso cref="IDeduplicatingBufferedQueue{TKey, TValue}"/>
public sealed class DeduplicatingBufferedQueue<TKey, TValue> : IDeduplicatingBufferedQueue<TKey, TValue>, IDisposable
{
    private readonly Func<TimeSpan> _CommitIntervalGetter;
    private readonly BlockingCollection<KeyValuePair<TKey, TValue>> _BlockingCollection;

    private ConcurrentDictionary<TKey, TValue> _CurrentDictionary;
    private Timer _CommitTimer;
    private bool _Disposed;

    /// <inheritdoc cref="IDeduplicatingBufferedQueue{TKey, TValue}.Count"/>
    public int Count => _BlockingCollection.Count + _CurrentDictionary.Count;

    /// <summary>
    /// Construct a new instance of <see cref="DeduplicatingBufferedQueue{TKey, TValue}"/>
    /// </summary>
    /// <param name="commitIntervalGetter">The function to get the commit interval.</param>
    public DeduplicatingBufferedQueue(Func<TimeSpan> commitIntervalGetter)
    {
        _CommitIntervalGetter = commitIntervalGetter;
        _CurrentDictionary = new ConcurrentDictionary<TKey, TValue>();
        _BlockingCollection = new BlockingCollection<KeyValuePair<TKey, TValue>>();

        var commitInterval = _CommitIntervalGetter();
        _CommitTimer = new Timer(s => PauseTimerAndCommit(), null, commitInterval, commitInterval);
    }

    /// <inheritdoc cref="IDeduplicatingBufferedQueue{TKey, TValue}.TryAdd(TKey, TValue)"/>
    public bool TryAdd(TKey key, TValue value) => _CurrentDictionary.TryAdd(key, value);

    /// <inheritdoc cref="IDeduplicatingBufferedQueue{TKey, TValue}.TryTake(out KeyValuePair{TKey, TValue})"/>
    public bool TryTake(out KeyValuePair<TKey, TValue> kvp) => _BlockingCollection.TryTake(out kvp);

    /// <inheritdoc cref="IDeduplicatingBufferedQueue{TKey, TValue}.TryTake(out KeyValuePair{TKey, TValue}, TimeSpan)"/>
    public bool TryTake(out KeyValuePair<TKey, TValue> kvp, TimeSpan timeout) => _BlockingCollection.TryTake(out kvp, timeout);

    /// <inheritdoc cref="IDeduplicatingBufferedQueue{TKey, TValue}.Take"/>
    public KeyValuePair<TKey, TValue> Take() => _BlockingCollection.Take();

    /// <inheritdoc cref="IDeduplicatingBufferedQueue{TKey, TValue}.TakeMultiple(int)"/>
    public IEnumerable<KeyValuePair<TKey, TValue>> TakeMultiple(int maxCount)
    {
        var result = new List<KeyValuePair<TKey, TValue>>(maxCount);

        int i = 0;
        while (i < maxCount && _BlockingCollection.TryTake(out var kvp))
        {
            result.Add(kvp);

            i++;
        }

        return result;
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        if (_Disposed) return;

        EnqueueAccumulatedItems();

        _CommitTimer?.Dispose();
        _CommitTimer = null;
        _Disposed = true;
    }

    internal void PauseTimerAndCommit()
    {
        _CommitTimer.Change(-1, -1);
        EnqueueAccumulatedItems();

        var commitInterval = _CommitIntervalGetter();
        _CommitTimer.Change(commitInterval, commitInterval);
    }

    internal void EnqueueAccumulatedItems()
    {
        var replacementDictionary = new ConcurrentDictionary<TKey, TValue>();
        foreach (var kvp in Interlocked.Exchange(ref _CurrentDictionary, replacementDictionary))
            _BlockingCollection.TryAdd(kvp);
    }
}
