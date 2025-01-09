using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Tests.TestBase;

namespace TableConverter.FileConverters.Tests.InputConverterTests;

public class CsvInputTestCases : InputConverterTestCasesBase
{
    /// <summary>
    ///     Test cases for successful conversion.
    /// </summary>
    protected override
        IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options, TableData ExpectedTableData)>
        SuccessfulTestCases { get; } =
    [
        (
            // Test case 1: Correct CSV file with comma delimiter
            "test_input_csv_1.csv",
            new ConverterHandlerCsvOptions
            {
                Header = true,
                Delimiter = ","
            },
            Utils.TestTableData
        ),
        (
            // Test case 2: Correct CSV file with ampersand delimiter
            "test_input_csv_2.csv",
            new ConverterHandlerCsvOptions
            {
                Header = true,
                Delimiter = "&"
            },
            Utils.TestTableData
        )
    ];

    /// <summary>
    ///     Test cases for failed conversion.
    /// </summary>
    protected override IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options)> FailTestCases { get; } =
    [
        (
            // Test case 1: Incorrect CSV file with extra column in a row
            "test_input_incorrect_csv_1.csv",
            new ConverterHandlerCsvOptions
            {
                Header = true,
                Delimiter = ","
            }
        ),
        (
            // Test case 2: Incorrect CSV file with random binary data as row
            "test_input_incorrect_csv_2.csv",
            new ConverterHandlerCsvOptions
            {
                Header = true,
                Delimiter = ","
            }
        )
    ];
}

// ReSharper disable once UnusedType.Global
/// <summary>
///     Test class for <see cref="ConverterHandlerCsvInput" />
///     Uses <see cref="CsvInputTestCases" /> as test cases.
///     The <see cref="InputConverterTestBase{TInputConverter,TInputConverterData}" /> is used to instantiate the test
///     cases for an input converter.
/// </summary>
public class CsvInputTest : InputConverterTestBase<ConverterHandlerCsvInput, CsvInputTestCases>;