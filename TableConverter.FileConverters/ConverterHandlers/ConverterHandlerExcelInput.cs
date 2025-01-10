using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.ConverterHandlers;

public class ConverterHandlerExcelInput : ConverterHandlerInputAbstract<ConverterHandlerBaseOptions>
{
    private XSSFWorkbook? ExcelWorkbook { get; set; }

    public override Result<TableData> ReadText(string text)
    {
        var headers = new List<string>();
        var rows = new List<string[]>();

        try
        {
            if (ExcelWorkbook == null) throw new Exception("Excel Workbook is not initialized");

            var sheet = ExcelWorkbook.GetSheetAt(0);

            foreach (IRow row in sheet)
                if (row.RowNum == 0)
                {
                    headers.AddRange(row.Cells.Select(cell => cell.ToString() ?? ""));
                }
                else
                {
                    var values = new List<string>();

                    for (var i = 0; i < headers.Count; i++)
                    {
                        var cell = row.GetCell(i);

                        if (cell == null)
                        {
                            values.Add("");
                            continue;
                        }

                        values.Add(cell.ToString() ?? "");
                    }

                    rows.Add(values.ToArray());
                }

            ExcelWorkbook.Close();
            ExcelWorkbook.Dispose();
            ExcelWorkbook = null;
        }
        catch (Exception ex)
        {
            return Result<TableData>.Failure(ex.Message);
        }

        return Result<TableData>.Success(new TableData(headers, rows));
    }

    public override Result<string> ReadFile(Stream? stream)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        try
        {
            ExcelWorkbook = new XSSFWorkbook(stream);
            
            if (ExcelWorkbook.NumberOfSheets == 0 || ExcelWorkbook.GetSheetAt(0).PhysicalNumberOfRows == 0)
                return Result<string>.Failure("Excel file is empty");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }

        return Result<string>.Success($"Excel files are not visible within this text box 😭{Environment.NewLine}");
    }
}