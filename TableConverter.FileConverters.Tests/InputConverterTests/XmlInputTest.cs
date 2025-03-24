using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Tests.TestBase;

namespace TableConverter.FileConverters.Tests.InputConverterTests;

public class XmlInputTestCases : InputConverterTestCasesBase
{
    /// <summary>
    ///     Test cases which are expected to be successful.
    /// </summary>
    protected override
        IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options, TableData ExpectedTableData)>
        SuccessfulTestCases { get; } =
    [
        // Test case 1: Test with a valid XML file.
        (
            "test_input_xml_1.xml",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        ),
        // Test case 2: Test with a valid XML file. Minified XML.
        (
            "test_input_xml_2.xml",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        )
    ];

    /// <summary>
    ///     Test cases which are expected to fail.
    /// </summary>
    protected override IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options)> FailTestCases { get; } =
    [
        // Test case 1: Test with an invalid XML file. No end tag.
        (
            "test_input_incorrect_xml_1.xml",
            new ConverterHandlerBaseOptions()
        ),
        // Test case 2: Test with an invalid XML file. No end tag. Minified XML.
        (
            "test_input_incorrect_xml_2.xml",
            new ConverterHandlerBaseOptions()
        ),
        // Test case 3: Test with an invalid XML file. No start tag.
        (
            "test_input_incorrect_xml_3.xml",
            new ConverterHandlerBaseOptions()
        ),
        // Test case 4: Test with an invalid XML file. No start tag. Minified XML.
        (
            "test_input_incorrect_xml_4.xml",
            new ConverterHandlerBaseOptions()
        ),
        // Test case 5: Test with an invalid XML file. No root.
        (
            "test_input_incorrect_xml_5.xml",
            new ConverterHandlerBaseOptions()
        ),
    ];
}

// ReSharper disable once UnusedType.Global
/// <summary>
///     Test class for <see cref="ConverterHandlerXmlInput" />
///     Uses <see cref="XmlInputTestCases" /> as test cases.
///     The <see cref="InputConverterTestBase{TInputConverter,TInputConverterData}" /> is used to instantiate the test
///     cases for an input converter.
/// </summary>
public class XmlInputTest : InputConverterTestBase<ConverterHandlerXmlInput, XmlInputTestCases>;