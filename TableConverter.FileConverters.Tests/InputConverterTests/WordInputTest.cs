using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Tests.TestBase;

namespace TableConverter.FileConverters.Tests.InputConverterTests;

public class WordInputTestCases : InputConverterTestCasesBase
{
    /// <summary>
    ///     Test cases which are expected to be successful.
    /// </summary>
    protected override
        IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options, TableData ExpectedTableData)>
        SuccessfulTestCases { get; } =
    [
        // Test case 1: Successful test case with default options.
        (
            "test_input_word_1.docx",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        ),
    ];

    /// <summary>
    ///     Test cases which are expected to fail.
    /// </summary>
    protected override IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options)> FailTestCases { get; } =
    [
        // Test case 1: Test case with a file that is corrupted.
        (
            "test_input_incorrect_word_1.docx",
            new ConverterHandlerBaseOptions()
        ),
    ];
}

// ReSharper disable once UnusedType.Global
/// <summary>
///     Test class for <see cref="ConverterHandlerWordInput" />
///     Uses <see cref="WordInputTestCases" /> as test cases.
///     The <see cref="InputConverterTestBase{TInputConverter,TInputConverterData}" /> is used to instantiate the test
///     cases for an input converter.
/// </summary>
public class WordInputTest : InputConverterTestBase<ConverterHandlerWordInput, WordInputTestCases>
{
    /// <summary>
    ///     Overrides the asynchronous
    ///     <see cref="InputConverterTestBase{TInputConverter,TInputConverterData}.TestInputFile_WithFailData" /> test method
    ///     to test the conversion of a file with unsuccessful data.
    ///     Due to <see cref="NPOI" /> being used for inputting the Word file, the test is expected to fail early in the
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
    ///     Due to <see cref="NPOI.XSSF" /> being used for inputting the Word file, the test is expected to fail early in the
    ///     process.
    /// </summary>
    public override async Task TestInputFileAsync_WithFailData(string fileName, ConverterHandlerBaseOptions options)
    {
        Handler.Options = options;

        await using var stream = GetFileStream(fileName);

        // The test is expected to fail here due to the incorrect data.
        var fileResult = await Handler.ReadFileAsync(stream);
        Assert.False(fileResult.IsSuccess,
            $"fileResult.IsSuccess is true. Should be false due to data being incorrect. Data: {fileResult.Value}");
    }

    /// <summary>
    ///     Overrides the
    ///     <see cref="InputConverterTestBase{TInputConverter,TInputConverterData}.TestInputFile_WithEmptyData" /> test method
    ///     to test the conversion of a file with empty data.
    ///     Due to <see cref="NPOI.XWPF" /> being used for inputting the Word file, the test is expected to fail early in the
    ///     process.
    /// </summary>
    [Theory]
    [InlineData("test_input_empty_word.docx")]
    public override void TestInputFile_WithEmptyData(string fileName)
    {
        base.TestInputFile_WithEmptyData(fileName);
    }
}