using TableConverter.FileConverters.ConverterHandlers;
using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.Tests.TestBase;

namespace TableConverter.FileConverters.Tests.InputConverterTests;

public class AspInputTestCases : InputConverterTestCasesBase
{
    protected override IReadOnlyList<object[]> TestCases { get; } = new List<object[]>
    {
        new object[]
        {
            "test_input_asp_1.asp",
            new ConverterHandlerBaseOptions(),
            Utils.TestTableData
        }
    };
}

// ReSharper disable once UnusedType.Global
public class AspInputTest : InputConverterTestBase<ConverterHandlerAspInput, AspInputTestCases>;