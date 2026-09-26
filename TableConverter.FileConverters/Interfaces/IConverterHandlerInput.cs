using TableConverter.Utilities;

namespace TableConverter.FileConverters.Interfaces;

/// <summary>
///     Interface for handling input operations for file conversion.
/// </summary>
public interface IConverterHandlerInput
{
    /// <summary>
    ///     Gets or sets the dynamic options for input conversion.
    /// </summary>
    public dynamic? Options { get; set; }

    /// <summary>
    ///     Reads a file and pushes its rows straight into <paramref name="sink" />, one row at a time.
    /// </summary>
    /// <param name="stream">The stream containing file data.</param>
    /// <param name="sink">The destination the parsed rows are written to.</param>
    /// <param name="progress">
    ///     Receives how far the read has got, or <see langword="null" /> if the caller does not want
    ///     progress. The unit is the handler's to choose: a handler that reads a stream as it arrives
    ///     reports bytes, while one that parses a whole table before pushing it reports rows.
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the read.</param>
    /// <returns>A task that represents the asynchronous operation, containing a result indicating success or failure.</returns>
    /// <remarks>
    ///     <para>
    ///         This is the only entry point an input handler implements. A large file is never held in
    ///         memory as a whole string, nor built into a whole table in memory, before it can be
    ///         stored: rows are parsed and pushed as they are read.
    ///     </para>
    ///     <para>
    ///         A handler owns the call order on <paramref name="sink" />: it must call
    ///         <see cref="ITableRowSink.BeginAsync" /> before its first row and
    ///         <see cref="ITableRowSink.CompleteAsync" /> after its last. Returning a failure without
    ///         completing the sink is what tells the destination to discard whatever was written.
    ///     </para>
    ///     <para>
    ///         Reporting through <paramref name="progress" /> is the handler's own affair: a handler that
    ///         cannot measure its work simply never reports. Reports should be throttled, because
    ///         <see cref="IProgress{T}" /> marshals each one onto the captured context.
    ///     </para>
    /// </remarks>
    public Task<Result> ReadStreamAsync(Stream? stream, ITableRowSink sink,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default);
}