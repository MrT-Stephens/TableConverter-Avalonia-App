using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.Tests.TestBase;
using TableConverter.Utilities;

namespace TableConverter.FileConverters.Tests.InputConverterTests;

public class AspInputTestCases : InputConverterTestCasesBase
{
    /// <summary>
    /// Test cases which should pass
    /// </summary>
    protected override
        IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options, TableData ExpectedTableData)>
        SuccessfulTestCases { get; } =
    [
        (
            // Test case 1: Correct input file
            "test_input_asp_1.asp",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        )
    ];

    /// <summary>
    ///     Test cases which should fail
    /// </summary>
    protected override IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options)> FailTestCases { get; } =
    [
        (
            // Test case 1: Incorrect input file with missing "Dim arr(4,15)"
            "test_input_incorrect_asp_1.asp",
            new ConverterHandlerBaseOptions()
        ),
        (
            // Test case 2: Incorrect input file with incorrect array size
            "test_input_incorrect_asp_2.asp",
            new ConverterHandlerBaseOptions()
        )
    ];
}

// ReSharper disable once UnusedType.Global
/// <summary>
///     Test class for <see cref="ConverterHandlerAspInput" />
///     Uses <see cref="AspInputTestCases" /> as test cases.
///     The <see cref="InputConverterTestBase{TInputConverter,TInputConverterData}" /> is used to instantiate the test
///     cases for an input converter.
/// </summary>
public class AspInputTest : InputConverterTestBase<ConverterHandlerAspInput, AspInputTestCases>;