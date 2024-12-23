using System.Collections.Immutable;
using TableConverter.DataGeneration.LocaleDataSetsBase.Definitions;

namespace TableConverter.DataGeneration.LocaleDataSetsBase;

public abstract class ScienceBase
{
    public abstract ImmutableArray<ScienceUnitDefinition> Unit { get; }
    
    public abstract ImmutableArray<ScienceChemicalElementDefinition> ChemicalElement { get; }
}