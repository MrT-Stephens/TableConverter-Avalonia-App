using System.Collections.Immutable;
using TableConverter.DataGeneration.Interfaces;

namespace TableConverter.DataGeneration.LocaleDataSetsBase;

public abstract class PhoneNumberBase
{
    public abstract ImmutableArray<IWeightedValue<string>> HumanPattern { get; }
    
    public abstract ImmutableArray<IWeightedValue<string>> InternationalPattern { get; }
    
    public abstract ImmutableArray<IWeightedValue<string>> NationalPattern { get; }
}