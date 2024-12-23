using System.Collections.Immutable;

namespace TableConverter.DataGeneration.LocaleDataSetsBase;

public abstract class LoremBase
{
    public abstract ImmutableArray<string> Word { get; }
}