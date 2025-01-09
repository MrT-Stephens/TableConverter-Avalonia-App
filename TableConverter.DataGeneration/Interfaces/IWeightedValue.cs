namespace TableConverter.DataGeneration.Interfaces;

/// <summary>
///     Interface for a weighted value.
///     Allows for retrieval of a value and its weight.
///     The weight is used to determine the probability of the value being selected.
/// </summary>
/// <typeparam name="T">The type of the value</typeparam>
public interface IWeightedValue<out T>
{
    public T Value { get; }
    public int Weight { get; }
}