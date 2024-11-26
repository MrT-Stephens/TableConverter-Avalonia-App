using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerExcelOutput : ConverterHandlerOutputAbstract<ConverterHandlerExcelOutputOptions>
    {
        private XSSFWorkbook? ExcelWorkbook { get; set; }

        public override Result<string> Convert(string[] headers, string[][] rows)
        {
            ExcelWorkbook = new XSSFWorkbook();

            var sheet = ExcelWorkbook.CreateSheet(string.IsNullOrEmpty(Options!.SheetName) ? "Sheet1" : Options!.SheetName);

            var headerRow = sheet.CreateRow(0);

            for (long i = 0; i < headers.LongLength; i++)
            {
                headerRow.CreateCell((int)i).SetCellValue(headers[i]);
            }

            for (long i = 0; i < rows.LongLength; i++)
            {
                var row = sheet.CreateRow((int)i + 1);

                for (long j = 0; j < headers.LongLength; j++)
                {
                    sheet.AutoSizeColumn((int)i);
                    row.CreateCell((int)j).SetCellValue(rows[i][j]);
                }
            }

            return Result<string>.Success($"Please save the '.xlsx' file to view the generated file 😁{Environment.NewLine}");
        }

        public override Result SaveFile(Stream? stream, ReadOnlyMemory<byte> buffer)
        {
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));

            try
            {
                ExcelWorkbook?.Write(stream);
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.Message);
            }
            
            return Result.Success();
        }
    }
}
