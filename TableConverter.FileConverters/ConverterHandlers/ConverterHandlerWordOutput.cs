using NPOI.XWPF.UserModel;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerWordOutput : ConverterHandlerOutputAbstract<ConverterHandlerBaseOptions>
{
    public override async Task<Result> ConvertToStreamAsync(
        Stream? stream,
        ITableRowSource source,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(source);

        try
        {
            var headers = await source.GetHeadersAsync(cancellationToken).ConfigureAwait(false);

            using var document = new XWPFDocument();

            // The table starts with the header row only and gains a row per row read, so its size does
            // not have to be counted out before the document can be built.
            var wordTable = document.CreateTable(1, headers.Count);

            for (var i = 0; i < headers.Count; i++)
                wordTable.GetRow(0).GetCell(i).SetText(headers[i] ?? string.Empty);

            await foreach (var cells in source.ReadTextRowsAsync(cancellationToken).ConfigureAwait(false))
            {
                // NPOI gives the new row as many cells as the table has columns.
                var wordRow = wordTable.CreateRow();

                for (var j = 0; j < headers.Count; j++)
                {
                    // Guard against ragged rows: the caller may supply fewer cells than there are headers.
                    var value = j < cells.Length ? cells[j] : string.Empty;

                    wordRow.GetCell(j).SetText(value);
                }
            }

            // NPOI closes the stream it is given, so wrap it to protect the caller owned stream.
            document.Write(new NonClosingStreamWrapper(stream));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}