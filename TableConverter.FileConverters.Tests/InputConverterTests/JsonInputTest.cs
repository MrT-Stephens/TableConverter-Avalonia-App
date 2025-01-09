using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Tests.TestBase;

namespace TableConverter.FileConverters.Tests.InputConverterTests;

public class JsonInputTestCases : InputConverterTestCasesBase
{
    protected override
        IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options, TableData ExpectedTableData)>
        SuccessfulTestCases { get; } =
    [
        (
            "test_input_json_1.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Array of Objects"
            },
            Utils.TestTableData
        ),
        (
            "test_input_json_2.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Array of Objects"
            },
            Utils.TestTableData
        ),
        (
            "test_input_json_3.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "2D Arrays"
            },
            Utils.TestTableData
        ),
        (
            "test_input_json_4.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "2D Arrays"
            },
            Utils.TestTableData
        ),
        (
            "test_input_json_5.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Column Arrays"
            },
            Utils.TestTableData
        ),
        (
            "test_input_json_6.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Column Arrays"
            },
            Utils.TestTableData
        ),
        (
            "test_input_json_7.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Keyed Arrays"
            },
            Utils.TestTableData
        ),
        (
            "test_input_json_8.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Keyed Arrays"
            },
            Utils.TestTableData
        )
    ];

    protected override IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options)> FailTestCases { get; } =
    [
        (
            "test_input_json_1.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Invalid Format"
            }
        ),
        (
            "test_input_json_1.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Keyed Arrays"
            }
        ),
        (
            "test_input_json_2.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Column Arrays"
            }
        ),
        (
            "test_input_json_3.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Column Arrays"
            }
        ),
        (
            "test_input_json_4.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Keyed Arrays"
            }
        ),
        (
            "test_input_json_5.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "2D Arrays"
            }
        ),
        (
            "test_input_json_6.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Array of Objects"
            }
        ),
        (
            "test_input_json_7.json",
            new ConverterHandlerJsonInputOptions
            {
                SelectedJsonFormatType = "Array of Objects"
            }
        ),
        (
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