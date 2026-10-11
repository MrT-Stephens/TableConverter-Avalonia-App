using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;
using TableConverter.Utilities.Models;

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
            var columns = await source.GetColumnsAsync(cancellationToken).ConfigureAwait(false);

            using var workbook = new XSSFWorkbook();

            var sheet = workbook.CreateSheet(string.IsNullOrEmpty(Options!.SheetName) ? "Sheet1" : Options!.SheetName);

            var headerRow = sheet.CreateRow(0);

            for (var i = 0; i < columns.Count; i++) headerRow.CreateCell(i).SetCellValue(columns[i].Name);

            // A date stored as a date is a number underneath, so it needs a format of its own to be shown
            // as a date rather than as the serial number a spreadsheet keeps it as.
            var dateStyle = CreateDateStyle(workbook, "yyyy-mm-dd");
            var dateTimeStyle = CreateDateStyle(workbook, "yyyy-mm-dd hh:mm:ss");

            // Rows are written as they are pulled from the source, so the table is never copied into a
            // second, whole-table structure before the workbook can accept it.
            var rowIndex = 1;

            await foreach (var cells in source.ReadTextRowsAsync(cancellationToken).ConfigureAwait(false))
            {
                var row = sheet.CreateRow(rowIndex++);

                for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
                {
                    // Guard against ragged rows: the caller may supply fewer cells than there are headers.
                    var value = columnIndex < cells.Length ? cells[columnIndex] : string.Empty;

                    SetCellValue(row.CreateCell(columnIndex), columns[columnIndex], value, dateStyle, dateTimeStyle);
                }
            }

            // Auto sizing needs the cells to exist first, and must be applied per column.
            // It relies on a font measurement backend (SkiaSharp) which may not be present in every
            // host, and it is only cosmetic, so a failure here must not fail the whole export.
            for (var j = 0; j < columns.Count; j++)
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

    private static ICellStyle CreateDateStyle(XSSFWorkbook workbook, string format)
    {
        var style = workbook.CreateCellStyle();
        style.DataFormat = workbook.CreateDataFormat().GetFormat(format);

        return style;
    }

    /// <summary>
    ///     Writes <paramref name="value" /> into <paramref name="cell" /> as the kind of value the column
    ///     holds, so a number lands in the workbook as a number and a date as a date rather than both as
    ///     text.
    /// </summary>
    /// <remarks>
    ///     A value that does not read as its column's type, and a column whose type is not known, are
    ///     written as the text they already are, so nothing is lost on the way out.
    /// </remarks>
    private static void SetCellValue(ICell cell, TableColumn column, string? value, ICellStyle dateStyle,
        ICellStyle dateTimeStyle)
    {
        if (column.TryReadValue(value, out var typed))
        {
            switch (typed)
            {
                case long integer:
                    cell.SetCellValue(integer);
                    return;

                case decimal number:
                    cell.SetCellValue((double)number);
                    return;

                case bool flag:
                    cell.SetCellValue(flag);
                    return;

                case DateOnly date:
                    cell.SetCellValue(date.ToDateTime(TimeOnly.MinValue));
                    cell.CellStyle = dateStyle;
                    return;

                case DateTime moment:
                    cell.SetCellValue(moment);
                    cell.CellStyle = dateTimeStyle;
                    return;
            }
        }

        cell.SetCellValue(value ?? string.Empty);
    }
}