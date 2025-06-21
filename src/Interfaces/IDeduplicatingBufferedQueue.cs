namespace Roblox.Collections.Interfaces;

using System;
using System.Collections.Generic;

/// <summary>
/// Interface for a deduplicating buffered queue.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of the value.</typeparam>
public interface IDeduplicatingBufferedQueue<TKey, TValue>
{
    /// <summary>Gets the count.</summary>
    /// <value>The count.</value>
    int Count { get; }

    /// <summary>Tries to add to the queue</summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns>True if the add suceeded</returns>
    bool TryAdd(TKey key, TValue value);

    /// <summary>
    /// Tries to take an element.
    /// </summary>
    /// <param name="kvp">The <see cref="KeyValuePair{TKey, TValue}"/>.</param>
    /// <returns>True if the take succeeded</returns>
    bool TryTake(out KeyValuePair<TKey, TValue> kvp);

    /// <summary>
    /// Tries to take an element.
    /// </summary>
    /// <param name="kvp">The <see cref="KeyValuePair{TKey, TValue}"/>.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns>True if the take succeeded</returns>
    bool TryTake(out KeyValuePair<TKey, TValue> kvp, TimeSpan timeout);

    /// <summary>
    /// Takes a key value pair.
    /// </summary>
    /// <returns>The <see cref="KeyValuePair{TKey, TValue}"/></returns>
    KeyValuePair<TKey, TValue> Take();

    /// <summary>
    /// Takes multiple key value pairs.
    /// </summary>
    /// <param name="maxCount">The maximum count.</param>
    /// <returns>An enumerable array of <see cref="KeyValuePair{TKey, TValue}"/></returns>
    IEnumerable<KeyValuePair<TKey, TValue>> TakeMultiple(int maxCount);
}
