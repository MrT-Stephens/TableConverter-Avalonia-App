namespace TableConverter.Utilities.Extensions;

public static class IEnumerableExtensions
{
    /// <summary>
    /// Executes the specified action on each element of the IEnumerable.
    /// </summary>
    /// <param name="source">
    /// The source IEnumerable to iterate over.
    /// </param>
    /// <param name="action">
    /// The action to execute on each element of the source IEnumerable.
    /// </param>
    /// <typeparam name="T">
    /// The type of elements in the source IEnumerable.
    /// </typeparam>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
        }
    }
}