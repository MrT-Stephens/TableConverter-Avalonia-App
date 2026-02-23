using System.Collections;
using System.Collections.ObjectModel;
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

    /// <summary>
    /// Converts an IEnumerable to an ObservableCollection.
    /// </summary>
    /// <param name="source">
    /// The source IEnumerable to convert.
    /// </param>
    /// <typeparam name="T">
    /// The type of elements in the source IEnumerable.
    /// </typeparam>
    /// <returns>
    /// An ObservableCollection containing the elements of the source IEnumerable.
    /// </returns>
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
    {
        return new ObservableCollection<T>(source);
    }

    /// <summary>
    /// Returns an empty IEnumerable if the source is null; otherwise, returns the source itself.
    /// </summary>
    /// <typeparam name="T">
    /// The type of elements in the source IEnumerable.
    /// </typeparam>
    /// <param name="source">
    /// The source IEnumerable which may be null.
    /// </param>
    /// <returns>
    /// An empty IEnumerable if the source is null; otherwise, the source itself.
    /// </returns>
    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source)
    {
        return source ?? Enumerable.Empty<T>();
    }

    /// <summary>
    /// Determines whether the IEnumerable is null or contains no elements.
    /// </summary>
    /// <typeparam name="T">
    /// The type of elements in the source IEnumerable.
    /// </typeparam>
    /// <param name="source">
    /// The source IEnumerable to check.
    /// </param>
    /// <returns>
    /// True if the source is null or contains no elements; otherwise, false.
    /// </returns>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return source == null || !source.Any();
    }
    
    /// <summary>
    /// Adds a range of items to the ICollection.
    /// </summary>
    /// <param name="collection">
    /// The ICollection to which items will be added.
    /// </param>
    /// <param name="items">
    /// The items to add to the ICollection.
    /// </param>
    /// <typeparam name="T">
    /// The type of elements in the ICollection.
    /// </typeparam>
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }
    
    /// <summary>
    /// Clears the ICollection and adds a range of items to it.
    /// </summary>
    /// <param name="collection">
    /// The ICollection to clear and which will receive new items.
    /// </param>
    /// <param name="items">
    /// The items to add to the ICollection after clearing it.
    /// </param>
    /// <typeparam name="T">
    /// The type of elements in the ICollection.
    /// </typeparam>
    public static void ClearAndAddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        collection.Clear();
        collection.AddRange(items);
    }
    
    /// <summary>
    /// Removes a range of items from the ICollection.
    /// </summary>
    /// <param name="collection">
    /// The ICollection from which items will be removed.
    /// </param>
    /// <param name="items">
    /// The items to remove from the ICollection.
    /// </param>
    /// <typeparam name="T">
    /// The type of elements in the ICollection.
    /// </typeparam>
    public static void RemoveRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            collection.Remove(item);
        }
    }
    
    /// <summary>
    /// Removes all items of type T from the ICollection.
    /// </summary>
    /// <param name="source">
    /// The source ICollection from which items will be removed.
    /// </param>
    /// <param name="item">
    /// The item of type T to remove from the ICollection.
    /// </param>
    /// <typeparam name="T">
    /// The type of elements to remove from the ICollection.
    /// </typeparam>
    public static T GetSingleOfType<T>(this IEnumerable source)
    {
        return source.OfType<T>().Single();
    }
    
    /// <summary>
    /// Replaces an item in the source collection with a new item.
    /// </summary>
    /// <param name="source">
    /// The source collection.
    /// </param>
    /// <param name="oldItem">
    /// The item to be replaced.
    /// </param>
    /// <param name="newItem">
    /// The new item to replace the old item with.
    /// </param>
    /// <typeparam name="T">
    /// The type of items in the source collection.
    /// </typeparam>
    /// <exception cref="ArgumentException">
    /// The old item was not found in the source collection.
    /// </exception>
    public static void Replace<T>(this IList<T> source, T oldItem, T newItem)
    {
        var index = source.IndexOf(oldItem);

        if (index == -1)
        {
            throw new ArgumentException("The old item was not found in the source collection.", nameof(oldItem));
        }

        source[index] = newItem;
    }
}