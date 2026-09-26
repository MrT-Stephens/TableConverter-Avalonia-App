using System.Data;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerXmlInput : ConverterHandlerInputAbstract<ConverterHandlerBaseOptions>
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
            using var dataSet = new DataSet();

            // ReadXml consumes the reader directly, so the document is parsed as it arrives instead of
            // being pulled into a string first.
            dataSet.ReadXml(reader);

            var table = dataSet.Tables[0];

            var headers = (from DataColumn column in table.Columns select column.ColumnName).ToList();

            await sink.BeginAsync(headers, cancellationToken).ConfigureAwait(false);

            foreach (DataRow row in table.Rows)
                await sink.WriteRowAsync(row.ItemArray.Select(x => x?.ToString() ?? "").ToArray(), cancellationToken)
                    .ConfigureAwait(false);

            await sink.CompleteAsync(cancellationToken).ConfigureAwait(false);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error reading XML file: '{ex.Message}'");
        }
    }
}