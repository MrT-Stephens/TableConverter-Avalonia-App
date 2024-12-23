using TableConverter.DataGeneration.Interfaces;

namespace TableConverter.DataGeneration.DataModels;

/// <summary>
/// A value with a weight.
/// Allows for a value to have an attached weight.
/// The weight is used to determine the probability of the value being selected.
/// </summary>
/// <param name="value">The value.</param>
/// <param name="weight">The weight of the value.</param>
/// <typeparam name="T">The type of the value.</typeparam>
public class WeightedValue<T>(T value, int weight) : IWeightedValue<T>
{
    public T Value { get; } = value;
    public int Weight { get; } = weight;
}