using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Tests.TestBase;

namespace TableConverter.FileConverters.Tests.InputConverterTests;

public class ExcelInputTestCases : InputConverterTestCasesBase
{
    /// <summary>
    ///     Test cases for successful test cases.
    /// </summary>
    protected override
        IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options, TableData ExpectedTableData)>
        SuccessfulTestCases { get; } =
    [
        (
            // Test case 1: Test input file with correct data.
            "test_input_excel_1.xlsx",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        )
    ];

    /// <summary>
    ///     Test cases for unsuccessful test cases.
    /// </summary>
    protected override IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options)> FailTestCases { get; } =
    [
        (
            // Test case 1: Test input file with incorrect data.
            "test_input_incorrect_excel_1.xlsx",
            new ConverterHandlerBaseOptions()
        )
    ];
}

// ReSharper disable once UnusedType.Global
/// <summary>
///     Test class for <see cref="ConverterHandlerExcelInput" />
///     Uses <see cref="ExcelInputTestCases" /> as test cases.
///     The <see cref="InputConverterTestBase{TInputConverter,TInputConverterData}" /> is used to instantiate the test
///     cases for an input converter.
/// </summary>
public class ExcelInputTest : InputConverterTestBase<ConverterHandlerExcelInput, ExcelInputTestCases>
{
    /// <summary>
    ///     Overrides the asynchronous
    ///     <see cref="InputConverterTestBase{TInputConverter,TInputConverterData}.TestInputFile_WithFailData" /> test method
    ///     to test the conversion of a file with unsuccessful data.
    ///     Due to <see cref="NPOI" /> being used for inputting the Excel file, the test is expected to fail early in the
    ///     process.
    /// </summary>
    public override void TestInputFile_WithFailData(string fileName, ConverterHandlerBaseOptions options)
    {
        Handler.Options = options;

        using var stream = GetFileStream(fileName);

        // The test is expected to fail here due to the incorrect data.
        var fileResult = Handler.ReadFile(stream);
        Assert.False(fileResult.IsSuccess,
            $"fileResult.IsSuccess is true. Should be false due to data being incorrect. Data: {fileResult.Value}");
    }

    /// <summary>
    ///     Overrides the asynchronous
    ///     <see cref="InputConverterTestBase{TInputConverter,TInputConverterData}.TestInputFileAsync_WithFailData" /> test
    ///     method to test the conversion of a file with unsuccessful data.
    ///     Due to <see cref="NPOI.XSSF" /> being used for inputting the Excel file, the test is expected to fail early in the
    ///     process.
    /// </summary>
    public override async void TestInputFileAsync_WithFailData(string fileName, ConverterHandlerBaseOptions options)
    {
        Handler.Options = options;

        await using var stream = GetFileStream(fileName);

        // The test is expected to fail here due to the incorrect data.
        var fileResult = await Handler.ReadFileAsync(stream);
        Assert.False(fileResult.IsSuccess,
            $"fileResult.IsSuccess is true. Should be false due to data being incorrect. Data: {fileResult.Value}");
    }
}