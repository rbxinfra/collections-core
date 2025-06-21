namespace Roblox.Collections;

using System.Collections.Generic;

/// <summary>
/// A base class for counters that includes the protected Commit method used to persist counts.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <seealso cref="ICounter{TKey}" />
public abstract class CounterBase<TKey> : ICounter<TKey>
{
    /// <summary>
    /// Commits the specified dictionary to the counter.
    /// </summary>
    /// <param name="committableDictionary">The dictionary.</param>
    protected virtual void Commit(IEnumerable<KeyValuePair<TKey, double>> committableDictionary)
    {
    }

    /// <summary>
    /// Increments the specified counter key.
    /// </summary>
    /// <param name="counterKey">The counter key.</param>
    /// <param name="amount">The amount.</param>
    public abstract void Increment(TKey counterKey, double amount = 1.0);
}
