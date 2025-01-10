using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Tests.TestBase;

namespace TableConverter.FileConverters.Tests.InputConverterTests;

public class MultiLineInputTestCases : InputConverterTestCasesBase
{
    /// <summary>
    ///     Test cases which are expected to be successful.
    /// </summary>
    protected override
        IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options, TableData ExpectedTableData)>
        SuccessfulTestCases { get; } =
    [
        (
            // Test case 1: Test with a row separator of "---"
            "test_input_multiline_1.txt",
            new ConverterHandlerMultiLineOptions()
            {
                RowSeparator = "---"
            },
            Utils.TestTableData
        ),
        (
            // Test case 2: Test with a row separator of "-(*)$%\u00a3@@@)(\u00a3$)\u00a3_$\u00a3$\u00a3\u00a3$"
            "test_input_multiline_2.txt",
            new ConverterHandlerMultiLineOptions()
            {
                RowSeparator = "-(*)$%\u00a3@@@)(\u00a3$)\u00a3_$\u00a3$\u00a3\u00a3$"
            },
            Utils.TestTableData
        ),
    ];

    /// <summary>
    ///     Test cases which are expected to fail.
    /// </summary>
    protected override IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options)> FailTestCases { get; } =
    [
        (
            // Test case 1: Test with an invalid row separator.
            "test_input_multiline_1.txt",
            new ConverterHandlerMultiLineOptions
            {
                RowSeparator = "InvalidRowSeparator"
            }
        ),
        (
            // Test case 2: Test with a row separator that is not found in the input text.
            "test_input_multiline_1.txt",
            new ConverterHandlerMultiLineOptions
            {
                RowSeparator = "-(*)$%\u00a3@@@)(\u00a3$)\u00a3_$\u00a3$\u00a3\u00a3$"
            }
        ),
        (
            // Test case 3: Test with a inconsistent column count in row 2.
            "test_input_incorrect_multiline_1.txt",
            new ConverterHandlerMultiLineOptions()
            {
                RowSeparator = "---"
            }
        )
    ];
}

// ReSharper disable once UnusedType.Global
/// <summary>
///     Test class for <see cref="ConverterHandlerMultiLineInput" />
///     Uses <see cref="MultiLineInputTestCases" /> as test cases.
///     The <see cref="InputConverterTestBase{TInputConverter,TInputConverterData}" /> is used to instantiate the test
///     cases for an input converter.
/// </summary>
public class MultiLineInputTest : InputConverterTestBase<ConverterHandlerMultiLineInput, MultiLineInputTestCases>;