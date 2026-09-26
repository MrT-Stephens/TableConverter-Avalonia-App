using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerHtmlOutput : ConverterHandlerOutputAbstract<ConverterHandlerHtmlOutputOptions>
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

        var tabCount = 0;

        writer.Write($"<table>{(Options!.MinifyHtml ? "" : Environment.NewLine + new string('\t', ++tabCount))}");

        if (Options!.IncludeTheadTbody)
            writer.Write(
                $"<thead>{(Options!.MinifyHtml ? "" : Environment.NewLine + new string('\t', ++tabCount))}");

        writer.Write($"<tr>{(Options!.MinifyHtml ? "" : Environment.NewLine)}");

        tabCount++;

        for (var i = 0; i < headers.Count; i++)
        {
            writer.Write($"{(Options!.MinifyHtml ? "" : new string('\t', tabCount))}<th>");
            writer.Write(headers[i]);
            writer.Write($"</th>{(Options!.MinifyHtml ? "" : Environment.NewLine)}");
        }

        writer.Write($"{(Options!.MinifyHtml ? "" : new string('\t', --tabCount))}</tr>");

        if (Options!.IncludeTheadTbody)
            writer.Write(
                $"{(Options!.MinifyHtml ? "" : Environment.NewLine + new string('\t', --tabCount))}</thead>");

        await foreach (var row in source.ReadTextRowsAsync(cancellationToken).ConfigureAwait(false))
        {
            if (Options!.IncludeTheadTbody)
                writer.Write(
                    $"{(Options!.MinifyHtml ? "" : Environment.NewLine + new string('\t', tabCount++))}<tbody>");

            writer.Write(
                $"{(Options!.MinifyHtml ? "" : Environment.NewLine + new string('\t', tabCount++))}<tr>");

            for (var j = 0; j < headers.Count; j++)
            {
                writer.Write(
                    $"{(Options!.MinifyHtml ? "" : Environment.NewLine + new string('\t', tabCount))}<td>");
                writer.Write(ConverterHandlerUtilities.GetCellValue(row, j));
                writer.Write("</td>");
            }

            writer.Write(
                $"{(Options!.MinifyHtml ? "" : Environment.NewLine + new string('\t', --tabCount))}</tr>");

            if (Options!.IncludeTheadTbody)
                writer.Write(
                    $"{(Options!.MinifyHtml ? "" : Environment.NewLine + new string('\t', --tabCount))}</tbody>");
        }

        writer.Write($"{(Options!.MinifyHtml ? "" : Environment.NewLine)}</table>");

        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}