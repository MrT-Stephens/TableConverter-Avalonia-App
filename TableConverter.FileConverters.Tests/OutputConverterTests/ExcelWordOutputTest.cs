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
    public void Convert_And_Save_Produces_A_Readable_Workbook()
    {
        var handler = CreateHandler();

        var result = handler.Convert(["Name", "Age"], [["Alice", "30"], ["Bob", "25"]]);

        Assert.True(result.IsSuccess, result.Error);

        using var stream = new MemoryStream();
        var saveResult = handler.SaveFile(stream, ReadOnlyMemory<byte>.Empty);

        Assert.True(saveResult.IsSuccess, saveResult.Error);

        stream.Position = 0;

        using var workbook = new XSSFWorkbook(stream);
        var sheet = workbook.GetSheetAt(0);

        Assert.Equal("Name", sheet.GetRow(0).GetCell(0).StringCellValue);
        Assert.Equal("Alice", sheet.GetRow(1).GetCell(0).StringCellValue);
        Assert.Equal("25", sheet.GetRow(2).GetCell(1).StringCellValue);
    }

    [Fact]
    public void Convert_Handles_Rows_With_Fewer_Cells_Than_Headers()
    {
        // Regression: this used to throw IndexOutOfRangeException.
        var handler = CreateHandler();

        var result = handler.Convert(["A", "B", "C"], [["1"]]);

        Assert.True(result.IsSuccess, result.Error);

        using var stream = new MemoryStream();
        Assert.True(handler.SaveFile(stream, ReadOnlyMemory<byte>.Empty).IsSuccess);

        stream.Position = 0;

        using var workbook = new XSSFWorkbook(stream);
        var row = workbook.GetSheetAt(0).GetRow(1);

        Assert.Equal("1", row.GetCell(0).StringCellValue);
        Assert.Equal(string.Empty, row.GetCell(1).StringCellValue);
    }

    [Fact]
    public void Convert_Writes_A_Row_For_Every_Input_Row()
    {
        var handler = CreateHandler();

        var rows = Enumerable.Range(0, 5)
            .Select(i => new[] { $"Name{i}", $"{i}" })
            .ToArray();

        Assert.True(handler.Convert(["Name", "Age"], rows).IsSuccess);

        using var stream = new MemoryStream();
        Assert.True(handler.SaveFile(stream, ReadOnlyMemory<byte>.Empty).IsSuccess);

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
    public void Convert_And_Save_Produces_A_Readable_Document()
    {
        var handler = CreateHandler();

        var result = handler.Convert(["Name", "Age"], [["Alice", "30"]]);

        Assert.True(result.IsSuccess, result.Error);

        using var stream = new MemoryStream();
        var saveResult = handler.SaveFile(stream, ReadOnlyMemory<byte>.Empty);

        Assert.True(saveResult.IsSuccess, saveResult.Error);

        stream.Position = 0;

        using var document = new XWPFDocument(stream);
        var table = document.Tables[0];

        Assert.Equal("Name", table.GetRow(0).GetCell(0).GetText());
        Assert.Equal("Alice", table.GetRow(1).GetCell(0).GetText());
    }

    [Fact]
    public void Convert_Handles_Rows_With_Fewer_Cells_Than_Headers()
    {
        // Regression: this used to throw IndexOutOfRangeException.
        var handler = CreateHandler();

        var result = handler.Convert(["A", "B", "C"], [["1"]]);

        Assert.True(result.IsSuccess, result.Error);

        using var stream = new MemoryStream();
        Assert.True(handler.SaveFile(stream, ReadOnlyMemory<byte>.Empty).IsSuccess);

        stream.Position = 0;

        using var document = new XWPFDocument(stream);
        var row = document.Tables[0].GetRow(1);

        Assert.Equal("1", row.GetCell(0).GetText());
        Assert.Equal(string.Empty, row.GetCell(1).GetText());
    }

    [Fact]
    public void Convert_Can_Be_Reused_For_Multiple_Batches()
    {
        // Regression: the previous document used to be leaked between conversions.
        var handler = CreateHandler();

        Assert.True(handler.Convert(["A"], [["1"]]).IsSuccess);
        Assert.True(handler.Convert(["A"], [["2"]]).IsSuccess);

        using var stream = new MemoryStream();
        Assert.True(handler.SaveFile(stream, ReadOnlyMemory<byte>.Empty).IsSuccess);

        stream.Position = 0;

        using var document = new XWPFDocument(stream);

        Assert.Equal("2", document.Tables[0].GetRow(1).GetCell(0).GetText());
    }
}

