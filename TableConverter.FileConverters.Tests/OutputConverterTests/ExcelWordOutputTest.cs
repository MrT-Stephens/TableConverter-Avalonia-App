using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;

namespace TableConverter.FileConverters.Tests.OutputConverterTests;

public class ExcelOutputTest
{
    private static ConverterHandlerExcelOutput CreateHandler()
    {
        return new ConverterHandlerExcelOutput
        {
            Options = new ConverterHandlerExcelOutputOptions
            {
                SheetName = "Sheet1"
            }
        };
    }

    [Fact]
    public async Task Convert_Produces_A_Readable_Workbook()
    {
        var handler = CreateHandler();

        using var stream = new MemoryStream();

        var result = await Utils.ConvertToStreamAsync(handler, ["Name", "Age"],
            [["Alice", "30"], ["Bob", "25"]], stream);

        Assert.True(result.IsSuccess, result.Error);

        stream.Position = 0;

        using var workbook = new XSSFWorkbook(stream);
        var sheet = workbook.GetSheetAt(0);

        Assert.Equal("Name", sheet.GetRow(0).GetCell(0).StringCellValue);
        Assert.Equal("Alice", sheet.GetRow(1).GetCell(0).StringCellValue);
        Assert.Equal("25", sheet.GetRow(2).GetCell(1).StringCellValue);
    }

    [Fact]
    public async Task Convert_Handles_Rows_With_Fewer_Cells_Than_Headers()
    {
        // Regression: this used to throw IndexOutOfRangeException.
        var handler = CreateHandler();

        using var stream = new MemoryStream();

        var result = await Utils.ConvertToStreamAsync(handler, ["A", "B", "C"], [["1"]], stream);

        Assert.True(result.IsSuccess, result.Error);

        stream.Position = 0;

        using var workbook = new XSSFWorkbook(stream);
        var row = workbook.GetSheetAt(0).GetRow(1);

        Assert.Equal("1", row.GetCell(0).StringCellValue);
        Assert.Equal(string.Empty, row.GetCell(1).StringCellValue);
    }

    [Fact]
    public async Task Convert_Writes_A_Row_For_Every_Input_Row()
    {
        var handler = CreateHandler();

        var rows = Enumerable.Range(0, 5)
            .Select(i => new[] { $"Name{i}", $"{i}" })
            .ToArray();

        using var stream = new MemoryStream();

        Assert.True((await Utils.ConvertToStreamAsync(handler, ["Name", "Age"], rows, stream)).IsSuccess);

        stream.Position = 0;

        using var workbook = new XSSFWorkbook(stream);
        var sheet = workbook.GetSheetAt(0);

        // header + 5 data rows
        Assert.Equal(6, sheet.PhysicalNumberOfRows);
    }
}

public class WordOutputTest
{
    private static ConverterHandlerWordOutput CreateHandler()
    {
        return new ConverterHandlerWordOutput();
    }

    [Fact]
    public async Task Convert_Produces_A_Readable_Document()
    {
        var handler = CreateHandler();

        using var stream = new MemoryStream();

        var result = await Utils.ConvertToStreamAsync(handler, ["Name", "Age"], [["Alice", "30"]], stream);

        Assert.True(result.IsSuccess, result.Error);

        stream.Position = 0;

        using var document = new XWPFDocument(stream);
        var table = document.Tables[0];

        Assert.Equal("Name", table.GetRow(0).GetCell(0).GetText());
        Assert.Equal("Alice", table.GetRow(1).GetCell(0).GetText());
    }

    [Fact]
    public async Task Convert_Handles_Rows_With_Fewer_Cells_Than_Headers()
    {
        // Regression: this used to throw IndexOutOfRangeException.
        var handler = CreateHandler();

        using var stream = new MemoryStream();

        var result = await Utils.ConvertToStreamAsync(handler, ["A", "B", "C"], [["1"]], stream);

        Assert.True(result.IsSuccess, result.Error);

        stream.Position = 0;

        using var document = new XWPFDocument(stream);
        var row = document.Tables[0].GetRow(1);

        Assert.Equal("1", row.GetCell(0).GetText());
        Assert.Equal(string.Empty, row.GetCell(1).GetText());
    }

    [Fact]
    public async Task Convert_Can_Be_Reused_For_Multiple_Batches()
    {
        var handler = CreateHandler();

        using var first = new MemoryStream();
        using var second = new MemoryStream();

        Assert.True((await Utils.ConvertToStreamAsync(handler, ["A"], [["1"]], first)).IsSuccess);
        Assert.True((await Utils.ConvertToStreamAsync(handler, ["A"], [["2"]], second)).IsSuccess);

        second.Position = 0;

        using var document = new XWPFDocument(second);

        Assert.Equal("2", document.Tables[0].GetRow(1).GetCell(0).GetText());
    }
}
