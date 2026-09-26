using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerYamlInput : ConverterHandlerInputAbstract<ConverterHandlerBaseOptions>
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
        var yamlData = new Dictionary<string, string>();
        var begun = false;

        int position = 0, lineNumber = 0;

        for (var line = (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false))?.Trim();
             !string.IsNullOrEmpty(line);
             line = (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false))?.Trim())
        {
            position += line.Length + 1;
            lineNumber += 1;

            if (line.StartsWith("---")) continue;

            if (line.StartsWith('-'))
            {
                if (yamlData.Count > 0)
                {
                    if (!begun)
                    {
                        headers = yamlData.Keys.ToList();

                        await sink.BeginAsync(headers, cancellationToken).ConfigureAwait(false);
                        begun = true;
                    }

                    if (yamlData.Count != headers.Count)
                        return Result.Failure(
                            $"Incorrect number of columns at char position '{position}' in line '{lineNumber}'");

                    await sink.WriteRowAsync(yamlData.Values.ToArray(), cancellationToken).ConfigureAwait(false);
                }

                yamlData = new Dictionary<string, string>();
            }
            else
            {
                var lineData = line.Split(':');

                if (lineData.Length == 2) yamlData.Add(lineData[0].Trim(), lineData[1].Trim());
            }
        }

        if (yamlData.Count > 0)
        {
            if (!begun)
            {
                headers = yamlData.Keys.ToList();

                await sink.BeginAsync(headers, cancellationToken).ConfigureAwait(false);
                begun = true;
            }

            if (yamlData.Count != headers.Count)
                return Result.Failure(
                    $"Incorrect number of columns at char position '{position}' in line '{lineNumber}'");

            await sink.WriteRowAsync(yamlData.Values.ToArray(), cancellationToken).ConfigureAwait(false);
        }

        if (!begun)
        {
            await sink.BeginAsync(headers, cancellationToken).ConfigureAwait(false);
        }

        await sink.CompleteAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}