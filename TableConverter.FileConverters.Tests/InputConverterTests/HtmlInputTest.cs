using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.Tests.TestBase;

namespace TableConverter.FileConverters.Tests.InputConverterTests;

public class HtmlInputTestCases : InputConverterTestCasesBase
{
    protected override IReadOnlyList<object[]> TestCases { get; } = new List<object[]>
    {
        new object[]
        {
            "test_input_html_1.html",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        },
        new object[]
        {
            "test_input_html_2.html",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        },
        new object[]
        {
            "test_input_html_3.html",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        },
        new object[]
        {
            "test_input_html_4.html",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        },
        new object[]
        {
            "test_input_html_5.html",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        }
    };
}

// ReSharper disable once UnusedType.Global
public class HtmlInputTest : InputConverterTestBase<ConverterHandlerHtmlInput, HtmlInputTestCases>;