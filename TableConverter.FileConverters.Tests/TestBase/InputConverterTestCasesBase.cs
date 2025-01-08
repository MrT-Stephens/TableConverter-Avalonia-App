using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.Tests.TestBase;

public abstract class InputConverterTestCasesBase<TInputConverterOptions>
    where TInputConverterOptions : ConverterHandlerBaseOptions
{
    protected abstract IReadOnlyDictionary<string, (TableData, TInputConverterOptions)> TestCases { get; }

    public IEnumerable<object[]> GetTestCases()
    {
        return TestCases.Select(x => new object[] 
            { 
                x.Key, 
                x.Value.Item1, 
                x.Value.Item2 
            });
    }
}