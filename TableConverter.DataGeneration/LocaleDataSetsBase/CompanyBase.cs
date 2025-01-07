using System.Collections.Immutable;
using TableConverter.DataGeneration.Interfaces;

namespace TableConverter.DataGeneration.LocaleDataSetsBase;

public abstract class CompanyBase
{
    public abstract ImmutableArray<string> BuzzAdjective { get; }

    public abstract ImmutableArray<string> BuzzNoun { get; }

    public abstract ImmutableArray<string> BuzzVerb { get; }

    public abstract ImmutableArray<string> Adjective { get; }

    public abstract ImmutableArray<string> Descriptor { get; }

    public abstract ImmutableArray<IWeightedValue<ITemplatedValueBuilder<FakerBase, LocaleBase>>> NamePattern { get; }

    public abstract ImmutableArray<string> Noun { get; }
    
    public abstract ImmutableArray<string> Category { get; }
    
    public abstract ImmutableArray<string> LegalEntitySuffix { get; }
}