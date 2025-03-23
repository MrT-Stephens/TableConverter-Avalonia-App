using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Tests.TestBase;

namespace TableConverter.FileConverters.Tests.InputConverterTests;

public class SqlInputTestCases : InputConverterTestCasesBase
{
    /// <summary>
    ///     Test cases which are expected to be successful.
    /// </summary>
    protected override
        IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options, TableData ExpectedTableData)>
        SuccessfulTestCases { get; } =
    [
        // Test case 1: Test with a valid Sql input file. No quotes. Has column names.
        (
            "test_input_sql_1.sql",
            new ConverterHandlerSQLInputOptions
            {
                SelectedQuoteType = "No Quotes",
                HasColumnNames = true
            },
            Utils.TestTableData
        ),
        // Test case 2: Test with a valid Sql input file. Double Quotes. Has column names.
        (
            "test_input_sql_2.sql",
            new ConverterHandlerSQLInputOptions
            {
                SelectedQuoteType = "Double Quotes (\")",
                HasColumnNames = true
            },
            Utils.TestTableData
        ),
        // Test case 3: Test with a valid Sql input file. MySQL Quotes. Has column names.
        (
            "test_input_sql_3.sql",
            new ConverterHandlerSQLInputOptions
            {
                SelectedQuoteType = "MySQL Quotes (`)",
                HasColumnNames = true
            },
            Utils.TestTableData
        ),
        // Test case 4: Test with a valid Sql input file. SQL Server Quotes. Has column names.
        (
            "test_input_sql_4.sql",
            new ConverterHandlerSQLInputOptions
            {
                SelectedQuoteType = "SQL Server Quotes ([])",
                HasColumnNames = true
            },
            Utils.TestTableData
        ),
    ];

    /// <summary>
    ///     Test cases which are expected to fail.
    /// </summary>
    protected override IReadOnlyList<(string FileName, ConverterHandlerBaseOptions Options)> FailTestCases { get; } =
    [
        // Test case 1: Test with a invalid Sql input file. No quotes. Has column names. Icorrect number of columns and values.
        (
            "test_input_incorrect_sql_1.sql",
            new ConverterHandlerSQLInputOptions
            {
                SelectedQuoteType = "No Quotes",
                HasColumnNames = true
            }
        ),
        (
            "test_input_sql_2.sql",
            new ConverterHandlerSQLInputOptions
            {
                SelectedQuoteType = "No Quotes",
                HasColumnNames = true
            }
        )
    ];
}

// ReSharper disable once UnusedType.Global
/// <summary>
///     Test class for <see cref="ConverterHandlerSQLInput" />
///     Uses <see cref="SqlInputTestCases" /> as test cases.
///     The <see cref="InputConverterTestBase{TInputConverter,TInputConverterData}" /> is used to instantiate the test
///     cases for an input converter.
/// </summary>
public class SqlInputTest : InputConverterTestBase<ConverterHandlerSQLInput, SqlInputTestCases>;