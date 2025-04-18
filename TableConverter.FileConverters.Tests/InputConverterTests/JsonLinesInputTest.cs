using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Tests.TestBase;

namespace TableConverter.FileConverters.Tests.InputConverterTests;

public class JsonLinesInputTestCases : InputConverterTestCasesBase
{
    /// <summary>
    ///     Test cases for successful test cases.
    /// </summary>
    protected override
        IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options, TableData ExpectedTableData)>
        SuccessfulTestCases { get; } =
    [
        (
            // Test case 1: Test with json lines in objects format.
            "test_input_jsonlines_1.jsonl",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        ),
        (
            // Test case 2: Test with json lines in array format.
            "test_input_jsonlines_2.jsonl",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        )
    ];

    /// <summary>
    ///     Test cases for fail test cases.
    /// </summary>
    protected override IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options)> FailTestCases { get; } =
    [
        (
            // Test case 1: Test objects format with missing "}"  
            "test_input_incorrect_jsonlines_1.jsonl",
            new ConverterHandlerBaseOptions()
        ),
        (
            // Test case 2: Test array format with missing "]"  
            "test_input_incorrect_jsonlines_2.jsonl",
            new ConverterHandlerBaseOptions()
        )
    ];
}

// ReSharper disable once UnusedType.Global
/// <summary>
///     Test class for <see cref="ConverterHandlerJsonLinesInput" />
///     Uses <see cref="JsonLinesInputTestCases" /> as test cases.
///     The <see cref="InputConverterTestBase{TInputConverter,TInputConverterData}" /> is used to instantiate the test
///     cases for an input converter.
/// </summary>
public class JsonLinesInputTest : InputConverterTestBase<ConverterHandlerJsonLinesInput, JsonLinesInputTestCases>;