namespace Roblox.Collections;

using System;
using System.Collections.Generic;

/// <summary>
/// Extension methods for partitioning lists.
/// </summary>
public static class Partitioner
{
    /// <summary>
    /// Split the specified <see cref="List{T}"/> into chunks.
    /// </summary>
    /// <typeparam name="T">The type of the elements in <see cref="List{T}"/></typeparam>
    /// <param name="allItems">The <see cref="List{T}"/></param>
    /// <param name="maxChunkSize">The max chunk size.</param>
    /// <returns>The chunks.</returns>
    public static IEnumerable<ICollection<T>> SplitIntoChunks<T>(this List<T> allItems, int maxChunkSize)
    {
        for (int i = 0; i < allItems.Count; i += maxChunkSize)
        {
            int length = Math.Min(maxChunkSize, allItems.Count - i);

            yield return allItems.GetRange(i, length);
        }

        yield break;
    }
}
