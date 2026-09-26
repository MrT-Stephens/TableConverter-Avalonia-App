using System.Text.RegularExpressions;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public partial class ConverterHandlerMultiLineInput : ConverterHandlerInputAbstract<ConverterHandlerMultiLineOptions>
{
    public override async Task<Result> ReadStreamAsync(
        Stream? stream,
        ITableRowSink sink,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var opened = TableRowStream.OpenTextReader(stream);

        if (opened.IsSuccess is false)
        {
            return Result.Failure(opened.Error!);
        }

        using var reader = opened.Value;

        // Validate row separator
        if (!SpecialCharacterRegex().IsMatch(Options!.RowSeparator))
            return Result.Failure(
                $"Invalid row separator '{Options.RowSeparator}'. It must contain only special characters.");

        var headers = new List<string>();
        var row = new List<string>();
        var firstLine = true;
        var rowNumber = 0;
        var sawSeparator = false;
        var begun = false;

        for (var line = (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false))?.Trim();
             !string.IsNullOrEmpty(line);
             line = (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false))?.Trim())
        {
            rowNumber++;

            if (line == Options.RowSeparator)
            {
                sawSeparator = true;

                if (firstLine)
                {
                    // The header block is closed by the first separator, so the columns are known here.
                    firstLine = false;

                    await sink.BeginAsync(headers, cancellationToken).ConfigureAwait(false);
                    begun = true;
                }
                else
                {
                    if (row.Count != headers.Count)
                        return Result.Failure(
                            $"Row {rowNumber - 1} has inconsistent column count. Expected {headers.Count}, found {row.Count}.\nRow data: {string.Join(", ", row)}");

                    await sink.WriteRowAsync(row.ToArray(), cancellationToken).ConfigureAwait(false);
                    row.Clear();
                }
            }
            else if (firstLine)
            {
                headers.Add(line);
            }
            else
            {
                row.Add(line);
            }
        }

        // A separator that never appears means the whole file was read as headers, which the old
        // up front check rejected rather than returning a table of column names with no rows.
        if (!sawSeparator)
            return Result.Failure($"Row separator '{Options.RowSeparator}' not found in the input text.");

        // Validate last row if not empty
        if (row.Count > 0)
        {
            if (row.Count != headers.Count)
                return Result.Failure(
                    $"Row {rowNumber} has inconsistent column count. Expected {headers.Count}, found {row.Count}.\nRow data: {string.Join(", ", row)}");

            await sink.WriteRowAsync(row.ToArray(), cancellationToken).ConfigureAwait(false);
        }

        if (!begun)
        {
            await sink.BeginAsync(headers, cancellationToken).ConfigureAwait(false);
        }

        await sink.CompleteAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }

    [GeneratedRegex(@"^[^\p{L}\p{N}\s]+$")]
    private static partial Regex SpecialCharacterRegex();
}