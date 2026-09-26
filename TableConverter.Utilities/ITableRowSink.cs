namespace TableConverter.Utilities;

/// <summary>
///     Receives a table one row at a time, in the order the rows are supplied.
/// </summary>
/// <remarks>
///     <para>
///         This is the destination half of the streaming contract. Importers push rows into it as they
///         are parsed, so a large file never has to be held in memory as a whole table (or as a whole
///         string) before the destination can accept it.
///     </para>
///     <para>
///         The members are called in a fixed order: <see cref="BeginAsync" /> once, then
///         <see cref="WriteRowAsync" /> zero or more times, then <see cref="CompleteAsync" /> once. An
///         implementation is free to treat everything before <see cref="CompleteAsync" /> as provisional
///         and discard it if <see cref="CompleteAsync" /> is never reached.
///     </para>
/// </remarks>
public interface ITableRowSink
{
    /// <summary>
    ///     Gets a value indicating whether <see cref="CompleteAsync" /> has been called.
    /// </summary>
    /// <remarks>
    ///     Everything handed over before <see cref="CompleteAsync" /> is provisional, so a caller checks
    ///     this before treating a write that reported success as one that was actually kept.
    /// </remarks>
    bool IsCompleted { get; }

    /// <summary>
    ///     Declares the columns of the table and readies the sink to receive rows.
    /// </summary>
    /// <param name="headers">
    ///     The column names, in the order the values of each row are supplied. May be empty, in which
    ///     case <see cref="WriteRowAsync" /> has nothing to store per row.
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <remarks>
    ///     Called exactly once, and always before the first <see cref="WriteRowAsync" /> call.
    /// </remarks>
    Task BeginAsync(IReadOnlyList<string> headers, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Writes one row of values.
    /// </summary>
    /// <param name="cells">
    ///     The values of the row, positionally matching the headers passed to <see cref="BeginAsync" />.
    ///     A row that is shorter than the header list is padded, and a <see langword="null" /> value is
    ///     stored as absent rather than as an empty string, so the two stay distinguishable.
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task WriteRowAsync(IReadOnlyList<string?> cells, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Signals that every row has been written and the table is ready to be used.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <remarks>
    ///     Called exactly once, after the last <see cref="WriteRowAsync" /> call.
    /// </remarks>
    Task CompleteAsync(CancellationToken cancellationToken = default);
}
