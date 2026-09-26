using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerCsvOutput : ConverterHandlerOutputAbstract<ConverterHandlerCsvOptions>
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

        try
        {
            var headers = await source.GetHeadersAsync(cancellationToken).ConfigureAwait(false);

            // The writer belongs to the caller, so the CSV writer must not close it.
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = Options!.Delimiter,
                NewLine = Environment.NewLine,
                HasHeaderRecord = Options!.IncludeHeader
            }, leaveOpen: true);

            // The header is written from the first row rather than up front, so an empty table produces
            // an empty file, which is what writing a list of records used to do.
            var wroteHeader = false;

            await foreach (var row in source.ReadTextRowsAsync(cancellationToken).ConfigureAwait(false))
            {
                if (!wroteHeader)
                {
                    if (Options!.IncludeHeader)
                    {
                        for (var j = 0; j < headers.Count; j++) csv.WriteField(headers[j]);

                        csv.NextRecord();
                    }

                    wroteHeader = true;
                }

                for (var j = 0; j < headers.Count; j++)
                    csv.WriteField(ConverterHandlerUtilities.GetCellValue(row, j));

                csv.NextRecord();
            }
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }

        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}