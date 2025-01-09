using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Tests.TestBase;

namespace TableConverter.FileConverters.Tests.InputConverterTests;

public class CsvInputTestCases : InputConverterTestCasesBase
{
    protected override
        IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options, TableData ExpectedTableData)>
        SuccessfulTestCases { get; } =
    [
        (
            "test_input_csv_1.csv",
            new ConverterHandlerCsvOptions
            {
                Header = true,
                Delimiter = ","
            },
            Utils.TestTableData
        ),
        (
            "test_input_csv_2.csv",
            new ConverterHandlerCsvOptions
            {
                Header = true,
                Delimiter = "&"
            },
            Utils.TestTableData
        )
    ];

    protected override IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options)> FailTestCases { get; } =
    [
        (
            "test_input_incorrect_csv_1.csv",
            new ConverterHandlerCsvOptions
            {
                Header = true,
                Delimiter = ","
            }
        ),
        (
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