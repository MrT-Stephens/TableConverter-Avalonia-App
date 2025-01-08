using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;
using TableConverter.FileConverters.Tests.TestBase;

namespace TableConverter.FileConverters.Tests;

public class CsvInputTestCases : InputConverterTestCasesBase<ConverterHandlerCsvOptions>
{
    protected override IReadOnlyDictionary<string, (TableData, ConverterHandlerCsvOptions)> TestCases { get; }
        = new Dictionary<string, (TableData, ConverterHandlerCsvOptions)>
        {
            {
                "test_input_csv_1.csv",
                (
                    new TableData(
                        ["FIRST_NAME", "LAST_NAME", "GENDER", "COUNTRY_CODE"],
                        [
                            ["Luxeena", "Binoy", "F", "GB"],
                            ["Lisa", "Allen", "F", "GB"],
                            ["Richard", "Wood", "M", "GB"],
                            ["Luke", "Murphy", "M", "GB"],
                            ["Adrian", "Heacock", "M", "GB"],
                            ["Elvinas", "Palubinskas", "M", "GB"],
                            ["Sian", "Turner", "F", "GB"],
                            ["Potar", "Potts", "M", "GB"],
                            ["Janis", "Chrisp", "F", "GB"],
                            ["Sarah", "Proffitt", "F", "GB"],
                            ["Calissa", "Noonan", "F", "GB"],
                            ["Andrew", "Connors", "M", "GB"],
                            ["Siann", "Tynan", "F", "GB"],
                            ["Olivia", "Parry", "F", "GB"]
                        ]),
                    new ConverterHandlerCsvOptions
                    {
                        Header = true,
                        Delimiter = ","
                    }
                )
            }
        };
}

// ReSharper disable once UnusedType.Global
public class CsvInputTest : InputConverterTestBase<ConverterHandlerCsvInput, CsvInputTestCases, ConverterHandlerCsvOptions>;