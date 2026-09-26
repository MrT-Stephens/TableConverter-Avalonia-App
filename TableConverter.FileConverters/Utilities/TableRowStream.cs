using System.Runtime.CompilerServices;
using System.Text;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.Utilities;

/// <summary>
///     Helpers shared by the converters that work a row at a time.
/// </summary>
/// <remarks>
///     The streaming entry points on the converter handlers all need the same plumbing: open a file as
///     text without taking the caller's stream down with it, and hand a set of already parsed rows to a
///     sink one at a time.
/// </remarks>
public static class TableRowStream
{
    /// <summary>
    ///     How many rows pass between progress reports.
    /// </summary>
    /// <remarks>
    ///     <see cref="IProgress{T}" /> marshals every report onto the captured context, so reporting a row
    ///     at a time would cost one dispatch per row on a large table and swamp whatever is drawing the
    ///     progress. Reporting every so many rows keeps the cost of reporting bounded by the size of the
    ///     table divided by this number.
    /// </remarks>
    private const int ProgressReportInterval = 1000;

    /// <summary>
    ///     UTF-8 without a byte order mark, which is what the converters have always emitted.
    /// </summary>
    public static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    /// <summary>
    ///     Creates a text writer over <paramref name="stream" /> that leaves the stream open when it is
    ///     disposed. The caller owns the stream.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="bufferSize">The size of the writer's buffer.</param>
    public static StreamWriter CreateTextWriter(Stream stream, int bufferSize = 1 << 16)
    {
        return new StreamWriter(stream, Utf8NoBom, bufferSize, leaveOpen: true);
    }

    /// <summary>
    ///     Creates a text reader over <paramref name="stream" /> that leaves the stream open when it is
    ///     disposed. The caller owns the stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="bufferSize">The size of the reader's buffer.</param>
    public static StreamReader CreateTextReader(Stream stream, int bufferSize = 1 << 16)
    {
        return new StreamReader(stream, Utf8NoBom, detectEncodingFromByteOrderMarks: true, bufferSize,
            leaveOpen: true);
    }

    /// <summary>
    ///     Opens <paramref name="stream" /> as UTF-8 text, refusing one that holds nothing but
    ///     whitespace.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="bufferSize">The size of the reader's buffer.</param>
    /// <returns>
    ///     A result carrying the reader, or a failure if no stream was supplied or the file is empty. The
    ///     caller owns the stream, so a successful reader must be disposed by the caller.
    /// </returns>
    /// <remarks>
    ///     A file that holds nothing at all, or nothing but whitespace, is not a table, so it is rejected
    ///     here rather than left for every parser to decide on. Telling the two apart means reading into
    ///     the reader, so a stream that can be rewound is checked up front and put back where it was; one
    ///     that cannot is handed straight back, and only a stream that holds nothing at all is caught.
    /// </remarks>
    public static Result<StreamReader> OpenTextReader(Stream? stream, int bufferSize = 1 << 16)
    {
        if (stream is null)
        {
            return Result<StreamReader>.Failure("No file was supplied to read.");
        }

        if (stream.CanSeek)
        {
            if (IsWhitespaceOnly(stream))
            {
                return Result<StreamReader>.Failure("File is empty");
            }

            return Result<StreamReader>.Success(CreateTextReader(stream, bufferSize));
        }

        // A stream that cannot be rewound cannot be probed without consuming it, so its first character
        // is looked at through the reader that is handed back, which is the only way to keep it.
        var reader = CreateTextReader(stream, bufferSize);

        if (reader.Peek() >= 0)
        {
            return Result<StreamReader>.Success(reader);
        }

        reader.Dispose();

        return Result<StreamReader>.Failure("File is empty");
    }

    /// <summary>
    ///     Reports whether <paramref name="stream" /> holds nothing but whitespace, leaving its position
    ///     where it was so it can still be read from the beginning afterwards.
    /// </summary>
    private static bool IsWhitespaceOnly(Stream stream)
    {
        var position = stream.Position;

        try
        {
            using var reader = CreateTextReader(stream);

            for (var character = reader.Read(); character >= 0; character = reader.Read())
            {
                if (!char.IsWhiteSpace((char)character))
                {
                    return false;
                }
            }

            return true;
        }
        finally
        {
            stream.Position = position;
        }
    }

    /// <summary>
    ///     Copies <paramref name="rows" /> into <paramref name="sink" /> a row at a time.
    /// </summary>
    /// <param name="headers">The column names of the table.</param>
    /// <param name="rows">The rows of the table, positionally matching <paramref name="headers" />.</param>
    /// <param name="sink">The destination to copy them to.</param>
    /// <param name="progress">
    ///     Receives the number of rows copied, or <see langword="null" /> if the caller does not want
    ///     progress. A parser that reaches for this is written against a sequence, so the total is
    ///     reported as unknown and a consumer draws the progress as indeterminate.
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the copy.</param>
    /// <remarks>
    ///     A parser that has to see a whole file before it can name the first column still ends up with
    ///     every row in memory. This exists so such a parser hands those rows to a sink one at a time
    ///     rather than building some other whole-table shape to pass around first.
    /// </remarks>
    public static async Task<Result> PushAsync(
        IReadOnlyList<string> headers,
        IEnumerable<string[]> rows,
        ITableRowSink sink,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(headers);
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(sink);

        await sink.BeginAsync(headers, cancellationToken).ConfigureAwait(false);

        var written = 0L;

        foreach (var row in rows)
        {
            await sink.WriteRowAsync(row, cancellationToken).ConfigureAwait(false);

            written++;

            if (written % ProgressReportInterval == 0)
            {
                progress?.Report(new ConversionProgress(written, null));
            }
        }

        await sink.CompleteAsync(cancellationToken).ConfigureAwait(false);

        // The last report is always made, so a consumer is left showing a finished job rather than
        // whichever interval boundary the table happened to end nearest.
        progress?.Report(new ConversionProgress(written, written));

        return Result.Success();
    }

    /// <summary>
    ///     Streams the rows of <paramref name="source" /> with an absent value written as an empty
    ///     string, which is how the converters have always treated a missing cell.
    /// </summary>
    /// <param name="source">The table to read.</param>
    /// <param name="cancellationToken">Token used to cancel the read.</param>
    /// <remarks>
    ///     This keeps a converter's writing routine identical whichever way it was entered: the store
    ///     can hold an absent value, but a file cannot, so it is flattened here rather than in every
    ///     handler.
    /// </remarks>
    public static async IAsyncEnumerable<string[]> ReadTextRowsAsync(
        this ITableRowSource source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        await foreach (var row in source.ReadRowsAsync(cancellationToken).ConfigureAwait(false))
        {
            yield return row.Select(cell => cell ?? string.Empty).ToArray();
        }
    }
}
