using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.Tests.TestBase;

public abstract class InputConverterTestCasesBase
{
    protected abstract IReadOnlyList<
        (
        string FileName,
        ConverterHandlerBaseOptions Options,
        TableData ExpectedTableData
        )> SuccessfulTestCases { get; }

    protected abstract IReadOnlyList<
        (
        string FileName,
        ConverterHandlerBaseOptions Options
        )> FailTestCases { get; }

    public IEnumerable<object[]> GetSuccessfulTestCases()
    {
        return SuccessfulTestCases.Select(val => new object[]
        {
            val.FileName,
            val.Options,
            val.ExpectedTableData
        });
    }

    public IEnumerable<object[]> GetFailTestCases()
    {
        return FailTestCases.Select(val => new object[]
        {
            val.FileName,
            val.Options
        });
    }
}