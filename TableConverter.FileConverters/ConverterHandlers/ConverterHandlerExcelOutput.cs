using NPOI.XSSF.UserModel;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerExcelOutput : ConverterHandlerOutputAbstract<ConverterHandlerExcelOutputOptions>
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

            using var workbook = new XSSFWorkbook();

            var sheet = workbook.CreateSheet(string.IsNullOrEmpty(Options!.SheetName) ? "Sheet1" : Options!.SheetName);

            var headerRow = sheet.CreateRow(0);

            for (var i = 0; i < headers.Count; i++) headerRow.CreateCell(i).SetCellValue(headers[i] ?? string.Empty);

            // Rows are written as they are pulled from the source, so the table is never copied into a
            // second, whole-table structure before the workbook can accept it.
            var rowIndex = 1;

            await foreach (var cells in source.ReadTextRowsAsync(cancellationToken).ConfigureAwait(false))
            {
                var row = sheet.CreateRow(rowIndex++);

                for (var columnIndex = 0; columnIndex < headers.Count; columnIndex++)
                {
                    // Guard against ragged rows: the caller may supply fewer cells than there are headers.
                    var value = columnIndex < cells.Length ? cells[columnIndex] : string.Empty;

                    row.CreateCell(columnIndex).SetCellValue(value);
                }
            }

            // Auto sizing needs the cells to exist first, and must be applied per column.
            // It relies on a font measurement backend (SkiaSharp) which may not be present in every
            // host, and it is only cosmetic, so a failure here must not fail the whole export.
            for (var j = 0; j < headers.Count; j++)
            {
                try
                {
                    sheet.AutoSizeColumn(j);
                }
                catch (Exception)
                {
                    // Ignore: the column keeps its default width, the data is still written correctly.
                }
            }

            // NPOI closes the stream it is given, so wrap it to protect the caller owned stream.
            workbook.Write(new NonClosingStreamWrapper(stream));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}