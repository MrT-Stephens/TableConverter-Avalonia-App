using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Tests.TestBase;

namespace TableConverter.FileConverters.Tests.InputConverterTests;

public class HtmlInputTestCases : InputConverterTestCasesBase
{
    protected override
        IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options, TableData ExpectedTableData)>
        SuccessfulTestCases { get; } =
    [
        (
            "test_input_html_1.html",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        ),
        (
            "test_input_html_2.html",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        ),
        (
            "test_input_html_3.html",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        ),
        (
            "test_input_html_4.html",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        ),
        (
            "test_input_html_5.html",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        )
    ];

    protected override IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options)> FailTestCases { get; } =
    [
        (
            "test_input_incorrect_html_1.html",
            new ConverterHandlerBaseOptions()
        ),
        (
            "test_input_incorrect_html_2.html",
            new ConverterHandlerBaseOptions()
        )
    ];
}

// ReSharper disable once UnusedType.Global
/// <summary>
///     Test class for <see cref="ConverterHandlerHtmlInput" />
///     Uses <see cref="HtmlInputTestCases" /> as test cases.
///     The <see cref="InputConverterTestBase{TInputConverter,TInputConverterData}" /> is used to instantiate the test
///     cases for an input converter.
/// </summary>
public class HtmlInputTest : InputConverterTestBase<ConverterHandlerHtmlInput, HtmlInputTestCases>;