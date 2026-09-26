using NPOI.XWPF.UserModel;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerWordInput : ConverterHandlerInputAbstract<ConverterHandlerBaseOptions>
{
    public override async Task<Result> ReadStreamAsync(
        Stream? stream,
        ITableRowSink sink,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(sink);

        try
        {
            using var document = new XWPFDocument(stream);

            if (document.Tables.Count == 0)
            {
                return Result.Failure("No tables found in the Word document");
            }

            var table = document.Tables[0];

            var headers = table.Rows[0].GetTableCells().Select(cell => cell.GetText()).ToList();

            await sink.BeginAsync(headers, cancellationToken).ConfigureAwait(false);

            for (var i = 1; i < table.Rows.Count; i++)
            {
                await sink.WriteRowAsync(table.Rows[i].GetTableCells().Select(cell => cell.GetText()).ToArray(),
                    cancellationToken).ConfigureAwait(false);
            }

            await sink.CompleteAsync(cancellationToken).ConfigureAwait(false);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}