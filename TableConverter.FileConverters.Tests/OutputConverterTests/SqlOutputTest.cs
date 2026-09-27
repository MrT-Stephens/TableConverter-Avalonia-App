using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.Utilities.Models;

namespace TableConverter.FileConverters.Tests.OutputConverterTests;

public class SqlOutputTest
{
    private static ConverterHandlerSQLOutput CreateHandler()
    {
        return new ConverterHandlerSQLOutput
        {
            Options = new ConverterHandlerSQLOutputOptions()
        };
    }

    [Fact]
    public async Task Writes_One_Insert_Per_Row()
    {
        var result = await Utils.ConvertToTextAsync(CreateHandler(), ["Name", "Age"], [["Alice", "30"]]);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Contains("INSERT INTO table_name (Name, Age) VALUES ('Alice', '30');", result.Value);
    }

    [Fact]
    public async Task Writes_Numbers_Unquoted_For_A_Typed_Column()
    {
        IReadOnlyList<TableColumn> columns =
        [
            new TableColumn("Name", ColumnDataType.Text),
            new TableColumn("Age", ColumnDataType.Integer)
        ];

        var result = await Utils.ConvertTypedToTextAsync(CreateHandler(), columns, [["Alice", "30"]]);

        Assert.True(result.IsSuccess, result.Error);

        // The age is a numeric literal, so it lands in a numeric column as a number rather than as text.
        Assert.Contains("VALUES ('Alice', 30);", result.Value);
    }

    [Fact]
    public async Task Escapes_Quotes_In_Text_Values()
    {
        var result = await Utils.ConvertToTextAsync(CreateHandler(), ["Name"], [["O'Brien"]]);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Contains("('O''Brien');", result.Value);
    }
}
