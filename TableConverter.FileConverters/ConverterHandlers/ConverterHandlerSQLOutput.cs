using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerSQLOutput : ConverterHandlerOutputAbstract<ConverterHandlerSQLOutputOptions>
{
    public override async Task<Result> ConvertToStreamAsync(
        Stream? stream,
        ITableRowSource source,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(source);

        await using var writer = TableRowStream.CreateTextWriter(stream);

        var headers = await source.GetHeadersAsync(cancellationToken).ConfigureAwait(false);

        var quoteType = Options!.QuoteTypes[Options!.SelectedQuoteType];
        var closingQuote = quoteType == "[" ? "]" : quoteType;

        var headersText = string.Join(", ", headers.Select(header =>
            $"{quoteType}{header.Replace(' ', '_')}{closingQuote}"));

        if (Options!.InsertMultiRowsAtOnce)
        {
            // The closing ';' and newline belong after the last row, which is only known once the rows
            // run out, so they are written after the loop rather than with the last row.
            var wroteAny = false;

            await foreach (var row in source.ReadTextRowsAsync(cancellationToken).ConfigureAwait(false))
            {
                var rowText = BuildRowText(row, headers.Count);

                if (!wroteAny)
                {
                    writer.Write("INSERT INTO " +
                                 $"{quoteType}" +
                                 $"{Options!.TableName.Replace(' ', '_')}" +
                                 $"{closingQuote} " +
                                 $"({headersText}) VALUES{Environment.NewLine} ({rowText})");

                    wroteAny = true;
                }
                else
                {
                    writer.Write($",{Environment.NewLine} ({rowText})");
                }
            }

            if (wroteAny)
            {
                writer.Write($";{Environment.NewLine}");
            }
        }
        else
        {
            await foreach (var row in source.ReadTextRowsAsync(cancellationToken).ConfigureAwait(false))
            {
                var rowText = BuildRowText(row, headers.Count);

                writer.Write($"INSERT INTO " +
                             $"{quoteType}" +
                             $"{Options!.TableName.Replace(' ', '_')}" +
                             $"{closingQuote} " +
                             $"({headersText}) VALUES ({rowText});" + Environment.NewLine);
            }
        }

        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }

    private static string BuildRowText(string[] row, int columnCount)
    {
        // Iterate the headers so a ragged row is padded rather than producing fewer values than columns.
        return string.Join(", ", Enumerable.Range(0, columnCount)
            .Select(j => $"\'{ConverterHandlerUtilities.GetCellValue(row, j).Replace("\'", "\'\'")}\'"));
    }
}