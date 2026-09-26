using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerAspOutput : ConverterHandlerOutputAbstract<ConverterHandlerBaseOptions>
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

        // The array is declared with its size up front, so the row count is needed before the first row
        // can be written and the rows are gathered here first.
        var rows = new List<string[]>();

        await foreach (var row in source.ReadTextRowsAsync(cancellationToken).ConfigureAwait(false))
            rows.Add(row);

        writer.Write($"Dim arr({headers.Count},{rows.Count + 1}){Environment.NewLine}");

        for (var i = 0; i < headers.Count; i++) writer.Write($"arr({i},0) = {headers[i]}{Environment.NewLine}");

        for (var i = 0; i < rows.Count; i++)
        for (var j = 0; j < headers.Count; j++)
            writer.Write(
                $"arr({j},{i + 1}) = {ConverterHandlerUtilities.GetCellValue(rows[i], j)}{Environment.NewLine}");

        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}