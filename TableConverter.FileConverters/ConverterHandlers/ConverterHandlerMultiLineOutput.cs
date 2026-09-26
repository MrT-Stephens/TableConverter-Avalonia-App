using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerMultiLineOutput : ConverterHandlerOutputAbstract<ConverterHandlerMultiLineOptions>
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

        foreach (var column in headers) writer.WriteLine(column);

        await foreach (var row in source.ReadTextRowsAsync(cancellationToken).ConfigureAwait(false))
        {
            if (Options!.RowSeparator != string.Empty) writer.WriteLine(Options!.RowSeparator);

            for (var j = 0; j < headers.Count; j++)
                writer.WriteLine(ConverterHandlerUtilities.GetCellValue(row, j));
        }

        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}