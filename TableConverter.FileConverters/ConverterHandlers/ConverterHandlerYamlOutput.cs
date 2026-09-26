using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerYamlOutput : ConverterHandlerOutputAbstract<ConverterHandlerBaseOptions>
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

        writer.Write($"---{Environment.NewLine}");

        await foreach (var row in source.ReadTextRowsAsync(cancellationToken).ConfigureAwait(false))
        {
            writer.Write($"-{Environment.NewLine}");

            for (var j = 0; j < headers.Count; j++)
                writer.Write(
                    $"    {headers[j].Replace(' ', '_')}: {ConverterHandlerUtilities.GetCellValue(row, j)}{Environment.NewLine}");
        }

        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}