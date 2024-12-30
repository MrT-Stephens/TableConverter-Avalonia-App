using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.Modules;

public class ColorModule(FakerBase faker, LocaleBase locale, Randomizer randomizer) : ModuleBase(faker, locale, randomizer)
{
    public override string ModuleName => "Color";

    public virtual string Human()
    {
        return Randomizer.GetRandomElement(Locale.Color.Value.HumanName);
    }
    
    public virtual string Space()
    {
        return Randomizer.GetRandomElement(Locale.Color.Value.Space);
    }
}