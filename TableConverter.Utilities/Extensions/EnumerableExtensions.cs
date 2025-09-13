using System.Text;

namespace TableConverter.Utilities.Extensions;

public static class EnumerableExtensions
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
    public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
    {
        var index = 0;
        source.ForEach(item =>  action(item, index++));
    }

    /// <summary>
    /// Converts an IEnumerable to a string representation with elements separated by the specified separator.
    /// </summary>
    /// <param name="source">
    /// The source IEnumerable to convert.
    /// </param>
    /// <param name="separator">
    /// The separator to use between elements in the resulting string. Default is a comma (",").
    /// </param>
    /// <typeparam name="T">
    /// The type of elements in the source IEnumerable.
    /// </typeparam>
    /// <returns>
    /// A string representation of the IEnumerable with elements separated by the specified separator.
    /// </returns>
    public static string ToStringList<T>(this IEnumerable<T> source, string separator = ",")
    {
        var sb = new StringBuilder();
        
        source.ForEach(value =>
        {
            sb.Append(value);
            sb.Append(separator);
        });
        
        return sb.ToString();
    }
}