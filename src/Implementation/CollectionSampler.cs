namespace Roblox.Collections;

using System.Collections.Generic;

using Random;

/// <summary>
/// Represents a sampler for <see cref="ICollection{T}"/>
/// </summary>
public class CollectionSampler
{
    private readonly IRandom _Random;

    /// <summary>
    /// Construct a new instance of <see cref="CollectionSampler"/>
    /// </summary>
    /// <param name="random">The <see cref="IRandom"/></param>
    public CollectionSampler(IRandom random)
    {
        _Random = random;
    }

    /// <summary>
    /// Gets the sample of the <see cref="IList{T}"/>
    /// </summary>
    /// <typeparam name="T">The type of the element of the <see cref="IList{T}"/></typeparam>
    /// <param name="elements">The <see cref="IList{T}"/></param>
    /// <param name="sampleSize">The sample size.</param>
    /// <returns>The sampled <see cref="IList{T}"/></returns>
    public IList<T> GetSample<T>(IList<T> elements, int sampleSize)
    {
        if (sampleSize >= elements.Count)
            return elements;

        int lastIndex = elements.Count;

        var sample = new List<T>();
        while (lastIndex > 0 && sample.Count < sampleSize)
        {
            lastIndex--;

            int randomLocation = _Random.Next(0, lastIndex);

            var randomElement = elements[randomLocation];
            elements[randomLocation] = elements[lastIndex];

            sample.Add(randomElement);
        }

        return sample;
    }
}
