using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.Modules;

public class ScienceModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : ModuleBase(faker, locale, randomizer)
{
    public override string ModuleName => "Science";

    public virtual string ChemicalElement()
    {
        var element = Randomizer.GetRandomElement(Locale.Science.Value.ChemicalElement);

        return $"Name: {element.Name}, Symbol: {element.Symbol}, Atomic Number: {element.AtomicNumber}";
    }

    public virtual string Unit()
    {
        var unit = Randomizer.GetRandomElement(Locale.Science.Value.Unit);

        return $"Name: {unit.Name}, Symbol: {unit.Symbol}";
    }
}