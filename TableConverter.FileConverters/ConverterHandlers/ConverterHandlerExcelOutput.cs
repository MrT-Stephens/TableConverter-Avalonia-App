using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers
{
    public class ConverterHandlerExcelOutput : ConverterHandlerOutputAbstract<ConverterHandlerExcelOutputOptions>
    {
        private IWorkbook? ExcelWorkbook { get; set; } = null;

        public override string Convert(string[] headers, string[][] rows)
        {
            ExcelWorkbook = new XSSFWorkbook();

            ISheet sheet = ExcelWorkbook.CreateSheet(string.IsNullOrEmpty(Options!.SheetName) ? "Sheet1" : Options!.SheetName);

            IRow header_row = sheet.CreateRow(0);

            for (long i = 0; i < headers.LongLength; i++)
            {
                header_row.CreateCell((int)i).SetCellValue(headers[i]);
            }

            for (long i = 0; i < rows.LongLength; i++)
            {
                IRow row = sheet.CreateRow((int)i + 1);

                for (long j = 0; j < headers.LongLength; j++)
                {
                    sheet.AutoSizeColumn((int)i);
                    row.CreateCell((int)j).SetCellValue(rows[i][j]);
                }
            }

            return $"Please save the '.xlsx' file to view the generated file 😁{Environment.NewLine}";
        }

        public override void SaveFile(Stream? stream, ReadOnlyMemory<byte> buffer)
        {
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));

            ExcelWorkbook?.Write(stream);

            stream.Close();
        }
    }
}
