using TableConverter.Utilities;

namespace TableConverter.FileConverters.Interfaces;

/// <summary>
///     Interface for handling output operations for file conversion.
/// </summary>
public interface IConverterHandlerOutput
{
    /// <summary>
    ///     Gets or sets the dynamic options for output conversion.
    /// </summary>
    public dynamic? Options { get; set; }

    /// <summary>
    ///     Pulls rows from <paramref name="source" /> and writes the converted table straight to
    ///     <paramref name="stream" />.
    /// </summary>
    /// <param name="stream">The stream to write the converted data to.</param>
    /// <param name="source">The table to convert, supplied one row at a time.</param>
    /// <param name="progress">
    ///     Receives how far the write has got, or <see langword="null" /> if the caller does not want
    ///     progress. The unit is the handler's to choose, most often the rows pulled from
    ///     <paramref name="source" />.
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the conversion.</param>
    /// <returns>A task that represents the asynchronous operation, containing a result indicating success or failure.</returns>
    /// <remarks>
    ///     <para>
    ///         This is the only entry point an output handler implements. A large table is never held in
    ///         memory as a whole, nor converted into one giant string, before it can be written out:
    ///         rows are pulled and written as they go.
    ///     </para>
    ///     <para>
    ///         The stream is written to but not closed: ownership stays with the caller.
    ///     </para>
    ///     <para>
    ///         Reporting through <paramref name="progress" /> is the handler's own affair: a handler that
    ///         cannot measure its work simply never reports. Reports should be throttled, because
    ///         <see cref="IProgress{T}" /> marshals each one onto the captured context.
    ///     </para>
    /// </remarks>
    public Task<Result> ConvertToStreamAsync(Stream? stream, ITableRowSource source,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default);
}