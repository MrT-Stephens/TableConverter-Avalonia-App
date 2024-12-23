using System.Collections.Immutable;
using TableConverter.DataGeneration.Interfaces;
using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.Modules;

public abstract class ModuleBase(FakerBase faker, LocaleBase locale, Randomizer randomizer)
{
    public abstract string ModuleName { get; }
    
    protected readonly FakerBase Faker = faker;
    
    protected readonly LocaleBase Locale = locale;
    
    protected readonly Randomizer Randomizer = randomizer;
}