using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Tests.TestBase;

namespace TableConverter.FileConverters.Tests.InputConverterTests;

public class HtmlInputTestCases : InputConverterTestCasesBase
{
    /// <summary>
    ///     Test cases that should pass.
    /// </summary>
    protected override
        IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options, TableData ExpectedTableData)>
        SuccessfulTestCases { get; } =
    [
        (
            // Test case 1: Test with a simple HTML table.
            "test_input_html_1.html",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        ),
        (
            // Test case 2: Test with a simple HTML table with <thead> and <tbody> tags.
            "test_input_html_2.html",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        ),
        (
            // Test case 3: Test with a simple HTML table minified.
            "test_input_html_3.html",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        ),
        (
            // Test case 4: Test with a simple HTML table with <thead> and <tbody> tags minified.
            "test_input_html_4.html",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        ),
        (
            // Test case 5: Test with a simple styled HTML table.
            "test_input_html_5.html",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        ),
        (
            // Test case 6: Test with a simple styled HTML table minified.
            "test_input_html_6.html",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        )
    ];

    /// <summary>
    ///     Test cases that should fail.
    /// </summary>
    protected override IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options)> FailTestCases { get; } =
    [
        (
            // Test case 1: Test with simple HTML table with missing </tbody> tag.
            "test_input_incorrect_html_1.html",
            new ConverterHandlerBaseOptions()
        ),
        (
            // Test case 2: Test with simple HTML table with missing </td> tag.
            "test_input_incorrect_html_2.html",
            new ConverterHandlerBaseOptions()
        ),
        (
            // Test case 3: Test with simple HTML table with missing </tbody> tag minified.
            "test_input_incorrect_html_3.html",
            new ConverterHandlerBaseOptions()
        ),
        (
            // Test case 4: Test with simple HTML table with missing </td> tag minified.
            "test_input_incorrect_html_4.html",
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