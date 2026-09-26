using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerPhpInput : ConverterHandlerInputAbstract<ConverterHandlerBaseOptions>
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

        var headers = new List<string>();
        var firstLine = true;
        var rowCount = 0;
        var begun = false;

        for (var line = (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false))?.Trim().Replace("\t", "");
             !string.IsNullOrEmpty(line);
             line = (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false))?.Trim().Replace("\t", ""))
            if (line.StartsWith("array(") && line.EndsWith(")"))
            {
                var values = line.Replace("array(", string.Empty).Replace(")", string.Empty).Split(",");

                values = values
                    .Select(val => val.Substring(val.IndexOf("=>", StringComparison.Ordinal) + 2).Trim('"'))
                    .ToArray();

                if (firstLine)
                {
                    headers.AddRange(values);
                    firstLine = false;

                    await sink.BeginAsync(headers, cancellationToken).ConfigureAwait(false);
                    begun = true;
                }
                else
                {
                    if (values.Length != headers.Count)
                        return Result.Failure($"Incorrect number of columns at row {rowCount}.");

                    if (!begun)
                    {
                        await sink.BeginAsync(headers, cancellationToken).ConfigureAwait(false);
                        begun = true;
                    }

                    await sink.WriteRowAsync(values, cancellationToken).ConfigureAwait(false);
                    rowCount++;
                }
            }

        if (!begun)
        {
            await sink.BeginAsync(headers, cancellationToken).ConfigureAwait(false);
        }

        await sink.CompleteAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}