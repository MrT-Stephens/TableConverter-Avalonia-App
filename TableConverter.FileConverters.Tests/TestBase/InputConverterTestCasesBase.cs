using System.Collections;

namespace TableConverter.FileConverters.Tests.TestBase;

public abstract class InputConverterTestCasesBase : IEnumerable<object[]>
{
    protected abstract IReadOnlyList<object[]> TestCases { get; }

    public IEnumerator<object[]> GetEnumerator()
    {
        return TestCases.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}