using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerJsonLinesOutput
    : ConverterHandlerOutputAbstract<ConverterHandlerJsonLinesOutputOptions>
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

        switch (Options!.SelectedJsonLinesFormatType)
        {
            case ConverterHandlerJsonLinesOutputOptions.JsonLinesStyles.Objects:
            {
                // Lines are separated by writing the newline before every line but the first, which is
                // the same output as leaving it off the last row but does not need the row count.
                var first = true;

                await foreach (var row in source.ReadTextRowsAsync(cancellationToken).ConfigureAwait(false))
                {
                    if (!first) writer.Write(Environment.NewLine);

                    first = false;

                    writer.Write("{");

                    for (var j = 0; j < headers.Count; j++)
                    {
                        writer.Write($"\"{headers[j]}\":\"{ConverterHandlerUtilities.GetCellValue(row, j)}\"");

                        if (j != headers.Count - 1) writer.Write(",");
                    }

                    writer.Write("}");
                }

                break;
            }
            case ConverterHandlerJsonLinesOutputOptions.JsonLinesStyles.Arrays:
            {
                // Write headers
                writer.Write("[");

                writer.Write(string.Join(",", headers.Select(column => $"\"{column}\"").ToArray()));

                writer.Write("]");

                writer.Write(Environment.NewLine);

                // Write rows
                var first = true;

                await foreach (var row in source.ReadTextRowsAsync(cancellationToken).ConfigureAwait(false))
                {
                    if (!first) writer.Write(Environment.NewLine);

                    first = false;

                    writer.Write("[");

                    writer.Write(string.Join(",", row.Select(str => $"\"{str}\"").ToArray()));

                    writer.Write("]");
                }

                break;
            }
        }

        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}