using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.Tests.TestBase;

namespace TableConverter.FileConverters.Tests.InputConverterTests;

public class JsonInputTestCases : InputConverterTestCasesBase
{
    protected override IReadOnlyList<object[]> TestCases { get; } = new List<object[]>
    {
        new object[]
        {
            "test_input_json_1.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Array of Objects"
            },
            Utils.TestTableData
        },
        new object[]
        {
            "test_input_json_2.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Array of Objects"
            },
            Utils.TestTableData
        },
        new object[]
        {
            "test_input_json_3.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "2D Arrays"
            },
            Utils.TestTableData
        },
        new object[]
        {
            "test_input_json_4.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "2D Arrays"
            },
            Utils.TestTableData
        },
        new object[]
        {
            "test_input_json_5.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Column Arrays"
            },
            Utils.TestTableData
        },
        new object[]
        {
            "test_input_json_6.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Column Arrays"
            },
            Utils.TestTableData
        },
        new object[]
        {
            "test_input_json_7.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Keyed Arrays"
            },
            Utils.TestTableData
        },
        new object[]
        {
            "test_input_json_8.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Keyed Arrays"
            },
            Utils.TestTableData
        }
    };
}

// ReSharper disable once UnusedType.Global
public class JsonInputTest : InputConverterTestBase<ConverterHandlerJsonInput, JsonInputTestCases>
{
    [Theory]
    [InlineData("test_input_json_1.json", "Keyed Arrays")]
    [InlineData("test_input_json_2.json", "Column Arrays")]
    [InlineData("test_input_json_3.json", "Column Arrays")]
    [InlineData("test_input_json_4.json", "Keyed Arrays")]
    [InlineData("test_input_json_5.json", "2D Arrays")]
    [InlineData("test_input_json_6.json", "Array of Objects")]
    [InlineData("test_input_json_7.json", "Array of Objects")]
    [InlineData("test_input_json_8.json", "2D Arrays")]
    public void TestIncorrectJsonFormatType(string fileName, string jsonFormatType)
    {
        var options = new ConverterHandlerJsonInputOptions
        {
            SelectedJsonFormatType = jsonFormatType
        };

        Handler.Options = options;

        using var stream = GetFileStream(fileName); // Retrieves the file stream from resources.

        // Perform file reading and conversion, asserting success at each stage.
        var fileResult = Handler.ReadFile(stream);
        Assert.True(fileResult.IsSuccess, $"fileResult.IsSuccess is false. Error: {fileResult.Error}");

        var convertResult = Handler.ReadText(fileResult.Value);
        Assert.False(convertResult.IsSuccess,
            $"convertResult.IsSuccess is true. Should be false due to incorrect format. Data: {convertResult.Value}");
    }
}