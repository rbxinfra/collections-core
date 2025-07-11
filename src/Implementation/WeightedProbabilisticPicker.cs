﻿namespace Roblox.Collections;

using System;
using System.Linq;
using System.Collections.Generic;

using Random;

/// <summary>
/// Helper for picking elements off collections.
/// </summary>
public class WeightedProbabilisticPicker
{
    private const double _DoublePrecision = 1E-06;

    private readonly IRandom _Random;

    /// <summary>
    /// Construct a new instance of <see cref="WeightedProbabilisticPicker"/>
    /// </summary>
    /// <param name="random">The <see cref="IRandom"/></param>
    public WeightedProbabilisticPicker(IRandom random)
    {
        _Random = random;
    }

    /// <summary>
    ///  This method picks one element from a list of elements probabilistically based on weights passed in. The weight for
    ///  any element cannot be 0.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="elements">Data elements from which picking needs to be done.</param>
    /// <param name="weights">The element will be picked probalistically based on weights. 0 weights are not supported.</param>
    /// <returns>Picked element.</returns>
    public T PickElement<T>(IList<T> elements, IList<int> weights)
        => PickElements(elements, weights, 1).FirstOrDefault();

    /// <summary>
    ///  This method picks one or more elements from a list of elements probabilistically based on weights passed in. The weight for
    ///  any element cannot be 0.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="elements">Data elements from which picking needs to be done.</param>
    /// <param name="weights">The elements will be picked probalistically based on weights. 0 weights are not supported.</param>
    /// <param name="numberOfElementsToPick">Number of elements to pick</param>
    /// <returns>Picked elements.</returns>
    public IReadOnlyCollection<T> PickElements<T>(IList<T> elements, IList<int> weights, int numberOfElementsToPick)
    {
        ValidateInput(elements, weights, numberOfElementsToPick);

        var pickedElements = new List<T>(numberOfElementsToPick);
        var scaledCumulativeWeights = GetScaledCumulativeWeights(weights);

        for (int i = 0; i < numberOfElementsToPick; i++)
        {
            int pickedIndex = PickNextIndex(scaledCumulativeWeights);
            if (pickedIndex >= 0 && pickedIndex < elements.Count)
                pickedElements.Add(elements[pickedIndex]);
        }

        return pickedElements;
    }

    private void ValidateInput<T>(IList<T> elements, IList<int> weights, int numberOfElementsToPick)
    {
        if (elements == null || elements.Count == 0)
            throw new ArgumentException("Arguments are incorrect. Elements list is null or empty.");
        if (weights == null || weights.Count == 0)
            throw new ArgumentException("Arguments are incorrect. Weights list is null or empty.");
        if (elements.Count != weights.Count)
            throw new ArgumentException("Arguments are incorrect. Number of elements and number of weights passed in are not equal.");
        if (numberOfElementsToPick <= 0)
            throw new ArgumentException(string.Format("Arguments are incorrect. {0} is not greater than zero.", nameof(numberOfElementsToPick)));
    }

    private int PickNextIndex(IList<double> scaledCumulativeWeights)
        => BinarySearch(scaledCumulativeWeights, GetNextRandom());

    private int BinarySearch(IList<double> data, double element)
    {
        int start = 0;
        int end = data.Count - 1;

        while (start <= end)
        {
            int mid = (start + end) / 2;

            if (AreEqual(data[mid], element) || (data[mid] > element && (mid == start || data[mid - 1] < element)))
                return mid;

            if (data[mid] > element)
                end = mid - 1;
            else
                start = mid + 1;
        }

        return -1;
    }

    private IList<double> GetScaledCumulativeWeights(IList<int> weights)
    {
        var scaledCumulativeWeights = new List<double>(weights.Count);
        var totalWeight = Sum(weights);
        var cumWeight = 0.0;

        foreach (int weight in weights)
        {
            cumWeight += weight;
            var cumWeightRatio = cumWeight / totalWeight;

            scaledCumulativeWeights.Add(cumWeightRatio);
        }

        return scaledCumulativeWeights;
    }

    private double Sum(IList<int> weights)
    {
        var totalWeight = 0.0;

        foreach (int weight in weights)
            totalWeight += weight;

        return totalWeight;
    }

    /// <summary>
    ///     Get a random number greater than 0 and less than or equal to 1.
    /// </summary>
    /// <returns>Random number greater than 0 and less than or equal to 1. </returns>
    private double GetNextRandom()
    {
        var rand = 0.0;
        while (AreEqual(rand, 0.0))
            rand = _Random.NextDouble();

        return rand;
    }

    private bool AreEqual(double d1, double d2) => Math.Abs(d1 - d2) <= _DoublePrecision;
}
