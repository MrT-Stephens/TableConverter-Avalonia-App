using TableConverter.Utilities.Models;

namespace TableConverter.Utilities;

/// <summary>
///     Supplies a table one row at a time, so an exporter can write a large table out without loading
///     all of it into memory first.
/// </summary>
/// <remarks>
///     This is the source half of the streaming contract. The columns have to be known before the first
///     row is emitted, which is why they are read through their own member rather than being yielded as
///     the first row.
/// </remarks>
public interface ITableRowSource
{
    /// <summary>
    ///     Gets the columns of the table, in the order the values of each row are supplied.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The columns, each naming the kind of value it holds. May be empty.</returns>
    /// <remarks>
    ///     The columns are read through their own member rather than being yielded as the first row,
    ///     because an exporter has to know what it is writing before it writes any of it: the type of a
    ///     column decides how its cells are written.
    /// </remarks>
    Task<IReadOnlyList<TableColumn>> GetColumnsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the rows of the table in the order they were stored.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    ///     The rows, positionally matching the columns returned by
    ///     <see cref="GetColumnsAsync" />. A value that was stored as absent comes back as
    ///     <see langword="null" />.
    /// </returns>
    /// <remarks>
    ///     Implementations are expected to fetch rows in pages so that the memory a read needs is
    ///     bounded by the page size rather than by the size of the table.
    /// </remarks>
    IAsyncEnumerable<string?[]> ReadRowsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
///     Reads a table's columns when only their names are wanted.
/// </summary>
public static class TableRowSourceExtensions
{
    /// <summary>
    ///     The names of the table's columns, in order.
    /// </summary>
    /// <param name="source">The table to read.</param>
    /// <param name="cancellationToken">Token used to cancel the read.</param>
    /// <remarks>
    ///     Most exporters write a table out as text and have no use for the type of a column; this keeps
    ///     them from having to say so.
    /// </remarks>
    public static async Task<IReadOnlyList<string>> GetHeadersAsync(this ITableRowSource source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        var columns = await source.GetColumnsAsync(cancellationToken).ConfigureAwait(false);

        return columns.Names();
    }
}
