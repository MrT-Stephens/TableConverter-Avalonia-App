using System.Collections.Immutable;
using TableConverter.DataGeneration.Interfaces;
using TableConverter.DataGeneration.LocaleDataSetsBase.Definitions;

namespace TableConverter.DataGeneration.LocaleDataSetsBase;

public abstract class CommerceBase
{
    public abstract ImmutableArray<string> Department { get; }

    public abstract CommerceProductNameDefinition ProductName { get; }

    public abstract ImmutableArray<ITemplatedValueBuilder<FakerBase, LocaleBase>> ProductDescription { get; }
}