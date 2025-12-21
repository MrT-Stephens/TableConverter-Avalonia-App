namespace TableConverter.Utilities.Virtualisation.Interfaces;

/// <summary>
/// Represents a provider of collection details.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public interface IItemsProvider<T>
{
    /// <summary>
    /// Fetches the total number of items available.
    /// </summary>
    /// <returns>The total number of items.</returns>
    int FetchCount();

    /// <summary>
    /// Fetches a range of items.
    /// </summary>
    /// <param name="startIndex">The start index.</param>
    /// <param name="pageCount">The size of the page to fetch</param>
    /// <param name="overallCount">The overall count</param>
    /// <returns>A list of the items.</returns>
    IList<T> FetchRange(int startIndex, int pageCount, out int overallCount);
}