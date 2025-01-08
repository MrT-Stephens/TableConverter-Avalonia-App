using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.Tests.TestBase;

namespace TableConverter.FileConverters.Tests.InputConverterTests;

public class ExcelInputTestCases : InputConverterTestCasesBase
{
    protected override IReadOnlyList<object[]> TestCases { get; } = new List<object[]>
    {
        new object[]
        {
            "test_input_excel_1.xlsx",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        }
    };
}

// ReSharper disable once UnusedType.Global
public class ExcelInputTest : InputConverterTestBase<ConverterHandlerExcelInput, ExcelInputTestCases>;