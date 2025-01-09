using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Tests.TestBase;

namespace TableConverter.FileConverters.Tests.InputConverterTests;

public class AspInputTestCases : InputConverterTestCasesBase
{
    protected override
        IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options, TableData ExpectedTableData)>
        SuccessfulTestCases { get; } =
    [
        (
            "test_input_asp_1.asp",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        )
    ];

    protected override IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options)> FailTestCases { get; } =
    [
        (
            "test_input_incorrect_asp_1.asp",
            new ConverterHandlerBaseOptions()
        ),
        (
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