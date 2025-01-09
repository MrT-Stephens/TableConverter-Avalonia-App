using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Tests.TestBase;

namespace TableConverter.FileConverters.Tests.InputConverterTests;

public class JsonInputTestCases : InputConverterTestCasesBase
{
    /// <summary>
    ///     Test cases for successful test cases.
    /// </summary>
    protected override
        IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options, TableData ExpectedTableData)>
        SuccessfulTestCases { get; } =
    [
        (
            // Test case 1: Test with json in array of objects format.
            "test_input_json_1.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Array of Objects"
            },
            Utils.TestTableData
        ),
        (
            // Test case 2: Test with json in array of objects format minified.
            "test_input_json_2.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Array of Objects"
            },
            Utils.TestTableData
        ),
        (
            // Test case 3: Test with json in 2D arrays format.
            "test_input_json_3.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "2D Arrays"
            },
            Utils.TestTableData
        ),
        (
            // Test case 4: Test with json in 2D arrays format minified.
            "test_input_json_4.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "2D Arrays"
            },
            Utils.TestTableData
        ),
        (
            // Test case 5: Test with json in column arrays format.
            "test_input_json_5.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Column Arrays"
            },
            Utils.TestTableData
        ),
        (
            // Test case 6: Test with json in column arrays format minified.
            "test_input_json_6.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Column Arrays"
            },
            Utils.TestTableData
        ),
        (
            // Test case 7: Test with json in keyed arrays format.
            "test_input_json_7.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Keyed Arrays"
            },
            Utils.TestTableData
        ),
        (
            // Test case 8: Test with json in keyed arrays format minified.
            "test_input_json_8.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Keyed Arrays"
            },
            Utils.TestTableData
        )
    ];

    /// <summary>
    ///     Test cases for fail test cases.
    /// </summary>
    protected override IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options)> FailTestCases { get; } =
    [
        (
            // Test case 1: Test with invalid json format type.
            "test_input_json_1.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Invalid Format"
            }
        ),
        (
            // Test case 2: Test with array of objects formatted file with keyed arrays format type.
            "test_input_json_1.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Keyed Arrays"
            }
        ),
        (
            // Test case 3: Test with array of objects formatted file with column arrays format type.
            "test_input_json_2.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Column Arrays"
            }
        ),
        (
            // Test case 4: Test with 2D arrays formatted file with column arrays format type.
            "test_input_json_3.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Column Arrays"
            }
        ),
        (
            // Test case 5: Test with 2D arrays formatted file with keyed arrays format type.
            "test_input_json_4.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Keyed Arrays"
            }
        ),
        (
            // Test case 6: Test with column arrays formatted file with 2D arrays format type.
            "test_input_json_5.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "2D Arrays"
            }
        ),
        (
            // Test case 7: Test with column arrays formatted file with array of objects format type.
            "test_input_json_6.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Array of Objects"
            }
        ),
        (
            // Test case 8: Test with keyed arrays formatted file with array of objects format type.
            "test_input_json_7.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Array of Objects"
            }
        ),
        (
            // Test case 9: Test with keyed arrays formatted file with 2D arrays format type.
            "test_input_json_8.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "2D Arrays"
            }
        )
    ];
}

// ReSharper disable once UnusedType.Global
/// <summary>
///     Test class for <see cref="ConverterHandlerJsonInput" />
///     Uses <see cref="JsonInputTestCases" /> as test cases.
///     The <see cref="InputConverterTestBase{TInputConverter,TInputConverterData}" /> is used to instantiate the test
///     cases for an input converter.
/// </summary>
public class JsonInputTest : InputConverterTestBase<ConverterHandlerJsonInput, JsonInputTestCases>;