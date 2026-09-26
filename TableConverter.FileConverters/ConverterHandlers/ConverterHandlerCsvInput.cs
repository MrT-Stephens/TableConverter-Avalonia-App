using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerCsvInput : ConverterHandlerInputAbstract<ConverterHandlerCsvOptions>
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

        try
        {
            using var csvReader = new CsvReader(reader,
                new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    DetectDelimiter = true,
                    Delimiter = Options!.Delimiter,
                    DetectColumnCountChanges = true,
                    TrimOptions = TrimOptions.Trim,
                });

            using var records = csvReader.GetRecords<dynamic>().GetEnumerator();

            var headers = new List<string>();

            if (records.MoveNext())
            {
                // The columns come from the first record, so the sink cannot be started until it is read.
                var first = (IDictionary<string, object>)records.Current!;

                if (Options!.IncludeHeader)
                    headers.AddRange(first.Keys);
                else
                    for (long i = 0; i < first.Keys.Count; i++)
                        headers.Add($"Column {i}");

                await sink.BeginAsync(headers, cancellationToken).ConfigureAwait(false);

                await WriteRowAsync(sink, first, cancellationToken).ConfigureAwait(false);

                while (records.MoveNext())
                {
                    var row = (IDictionary<string, object>)records.Current!;

                    await WriteRowAsync(sink, row, cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                await sink.BeginAsync(headers, cancellationToken).ConfigureAwait(false);
            }

            await sink.CompleteAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }

    private static Task WriteRowAsync(
        ITableRowSink sink,
        IEnumerable<KeyValuePair<string, object>> row,
        CancellationToken cancellationToken)
    {
        return sink.WriteRowAsync(row.Select(x => x.Value?.ToString() ?? string.Empty).ToArray(), cancellationToken);
    }
}