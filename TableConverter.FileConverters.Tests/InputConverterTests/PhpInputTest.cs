using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Tests.TestBase;

namespace TableConverter.FileConverters.Tests.InputConverterTests;

public class PhpInputTestCases : InputConverterTestCasesBase
{
    /// <summary>
    ///     Test cases which are expected to be successful.
    /// </summary>
    protected override
        IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options, TableData ExpectedTableData)>
        SuccessfulTestCases { get; } =
    [
        // Test case 1: Test with a valid PHP input file.
        (
            "test_input_php_1.txt",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        )
    ];

    /// <summary>
    ///     Test cases which are expected to fail.
    /// </summary>
    protected override IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options)> FailTestCases { get; } =
    [
        // Test case 1: Test with an incorrect PHP input file. Incorrect number of columns.
        (
            "test_input_incorrect_php_1.txt",   
            new ConverterHandlerBaseOptions()
        )
    ];
}

// ReSharper disable once UnusedType.Global
/// <summary>
///     Test class for <see cref="ConverterHandlerPhpInput" />
///     Uses <see cref="PhpInputTestCases" /> as test cases.
///     The <see cref="InputConverterTestBase{TInputConverter,TInputConverterData}" /> is used to instantiate the test
///     cases for an input converter.
/// </summary>
public class PhpInputTest : InputConverterTestBase<ConverterHandlerPhpInput, PhpInputTestCases>;