using NPOI.XSSF.UserModel;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Utilities;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerExcelOutput : ConverterHandlerOutputAbstract<ConverterHandlerExcelOutputOptions>
{
    private XSSFWorkbook? ExcelWorkbook { get; set; }

    public override Result<string> Convert(string[] headers, string[][] rows)
    {
        ExcelWorkbook?.Close();
        ExcelWorkbook = new XSSFWorkbook();

        var sheet = ExcelWorkbook.CreateSheet(string.IsNullOrEmpty(Options!.SheetName) ? "Sheet1" : Options!.SheetName);

        var headerRow = sheet.CreateRow(0);

        for (long i = 0; i < headers.LongLength; i++) headerRow.CreateCell((int)i).SetCellValue(headers[i]);

        for (long i = 0; i < rows.LongLength; i++)
        {
            var row = sheet.CreateRow((int)i + 1);

            for (long j = 0; j < headers.LongLength; j++)
            {
                // Guard against ragged rows: the caller may supply fewer cells than there are headers.
                var value = j < rows[i].LongLength ? rows[i][j] : string.Empty;

                row.CreateCell((int)j).SetCellValue(value);
            }
        }

        // Auto sizing needs the cells to exist first, and must be applied per column.
        // It relies on a font measurement backend (SkiaSharp) which may not be present in every
        // host, and it is only cosmetic, so a failure here must not fail the whole export.
        for (long j = 0; j < headers.LongLength; j++)
        {
            try
            {
                sheet.AutoSizeColumn((int)j);
            }
            catch (Exception)
            {
                // Ignore: the column keeps its default width, the data is still written correctly.
            }
        }

        return Result<string>.Success(
            $"Please save the '.xlsx' file to view the generated file 😁{Environment.NewLine}");
    }

    public override Result SaveFile(Stream? stream, ReadOnlyMemory<byte> buffer)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        try
        {
            // NPOI closes the stream it is given, so wrap it to protect the caller owned stream.
            ExcelWorkbook?.Write(new NonClosingStreamWrapper(stream));

            ExcelWorkbook?.Close();
            ExcelWorkbook = null;
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }
}