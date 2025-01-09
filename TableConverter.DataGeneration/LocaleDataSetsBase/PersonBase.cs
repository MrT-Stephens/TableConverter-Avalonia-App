using System.Collections.Immutable;
using TableConverter.DataGeneration.Interfaces;
using TableConverter.DataGeneration.LocaleDataSetsBase.Definitions;

namespace TableConverter.DataGeneration.LocaleDataSetsBase;

public abstract class PersonBase
{
    public abstract SexDefinition FirstName { get; }

    public abstract SexDefinition MiddleName { get; }

    public abstract SexDefinition LastName { get; }

    public abstract SexDefinition Title { get; }

    public abstract ImmutableArray<string> Suffix { get; }

    public abstract ImmutableArray<IWeightedValue<ITemplatedValueBuilder<FakerBase, LocaleBase>>> NamePattern { get; }

    public abstract ImmutableArray<IWeightedValue<ITemplatedValueBuilder<FakerBase, LocaleBase>>> LastNamePattern
    {
        get;
    }

    public abstract ImmutableArray<string> Gender { get; }

    public abstract ImmutableArray<string> JobType { get; }

    public abstract ImmutableArray<string> JobArea { get; }

    public abstract ImmutableArray<string> JobDescriptor { get; }

    public abstract ImmutableArray<IWeightedValue<ITemplatedValueBuilder<FakerBase, LocaleBase>>> JobTypePattern
    {
        get;
    }

    public abstract ImmutableArray<string> BiographyPart { get; }

    public abstract ImmutableArray<string> BiographySupporter { get; }

    public abstract ImmutableArray<IWeightedValue<ITemplatedValueBuilder<FakerBase, LocaleBase>>> BiographyPattern
    {
        get;
    }
}