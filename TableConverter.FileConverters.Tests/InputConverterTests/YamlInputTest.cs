using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Tests.TestBase;

namespace TableConverter.FileConverters.Tests.InputConverterTests;

public class YamlInputTestCases : InputConverterTestCasesBase
{
    /// <summary>
    ///     Test cases which are expected to be successful.
    /// </summary>
    protected override
        IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options, TableData ExpectedTableData)>
        SuccessfulTestCases { get; } =
    [
        // Test Case 1: Successful data.
        (
            "test_input_yaml_1.txt",    
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        ),
        // Test Case 2: Successful data.
        (
            "test_input_yaml_2.txt",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        ),
    ];

    /// <summary>
    ///     Test cases which are expected to fail.
    /// </summary>
    protected override IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options)> FailTestCases { get; } =
    [
        // Test Case 1: Unsuccessful data. Missing column.
        (
            "test_input_incorrect_yaml_1.txt",
            new ConverterHandlerBaseOptions()
        ),
    ];
}

// ReSharper disable once UnusedType.Global
/// <summary>
///     Test class for <see cref="ConverterHandlerYamlInput" />
///     Uses <see cref="YamlInputTestCases" /> as test cases.
///     The <see cref="InputConverterTestBase{TInputConverter,TInputConverterData}" /> is used to instantiate the test
///     cases for an input converter.
/// </summary>
public class YamlInputTest : InputConverterTestBase<ConverterHandlerYamlInput, YamlInputTestCases>;