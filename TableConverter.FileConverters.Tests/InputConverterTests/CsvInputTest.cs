using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.Tests.TestBase;

namespace TableConverter.FileConverters.Tests.InputConverterTests;

public class CsvInputTestCases : InputConverterTestCasesBase
{
    protected override IReadOnlyList<object[]> TestCases { get; } = new List<object[]>
    {
        new object[]
        {
            "test_input_csv_1.csv",
            new ConverterHandlerCsvOptions
            {
                Header = true,
                Delimiter = ","
            },
            Utils.TestTableData
        }
    };
}

// ReSharper disable once UnusedType.Global
public class CsvInputTest : InputConverterTestBase<ConverterHandlerCsvInput, CsvInputTestCases>;