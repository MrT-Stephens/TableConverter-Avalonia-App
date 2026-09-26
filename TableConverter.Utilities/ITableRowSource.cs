namespace TableConverter.Utilities;

/// <summary>
///     Supplies a table one row at a time, so an exporter can write a large table out without loading
///     all of it into memory first.
/// </summary>
/// <remarks>
///     This is the source half of the streaming contract. The headers have to be known before the first
///     row is emitted, which is why they are read through their own member rather than being yielded as
///     the first row.
/// </remarks>
public interface ITableRowSource
{
    /// <summary>
    ///     Gets the column names, in the order the values of each row are supplied.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The column names. May be empty.</returns>
    Task<IReadOnlyList<string>> GetHeadersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the rows of the table in the order they were stored.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    ///     The rows, positionally matching the headers returned by <see cref="GetHeadersAsync" />. A
    ///     value that was stored as absent comes back as <see langword="null" />.
    /// </returns>
    /// <remarks>
    ///     Implementations are expected to fetch rows in pages so that the memory a read needs is
    ///     bounded by the page size rather than by the size of the table.
    /// </remarks>
    IAsyncEnumerable<string?[]> ReadRowsAsync(CancellationToken cancellationToken = default);
}

