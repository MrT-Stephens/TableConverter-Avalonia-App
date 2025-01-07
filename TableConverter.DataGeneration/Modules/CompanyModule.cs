using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.Modules;

public class CompanyModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : ModuleBase(faker, locale, randomizer)
{
    public override string ModuleName => "Company";

    public virtual string Name()
    {
        return Randomizer.GetWeightedValue(Locale.Company.Value.NamePattern).Build(Faker, Locale, Randomizer);
    }

    public virtual string CatchPhrase()
    {
        return $"{CatchPhraseAdjective()} {CatchPhraseDescriptor()} {CatchPhraseNoun()}";
    }

    public virtual string BuzzPhrase()
    {
        return $"{BuzzVerb()} {BuzzAdjective()} {BuzzNoun()}";
    }

    public virtual string CatchPhraseAdjective()
    {
        return Randomizer.GetRandomElement(Locale.Company.Value.Adjective);
    }

    public virtual string CatchPhraseDescriptor()
    {
        return Randomizer.GetRandomElement(Locale.Company.Value.Descriptor);
    }

    public virtual string CatchPhraseNoun()
    {
        return Randomizer.GetRandomElement(Locale.Company.Value.Noun);
    }

    public virtual string BuzzAdjective()
    {
        return Randomizer.GetRandomElement(Locale.Company.Value.BuzzAdjective);
    }

    public virtual string BuzzVerb()
    {
        return Randomizer.GetRandomElement(Locale.Company.Value.BuzzVerb);
    }

    public virtual string BuzzNoun()
    {
        return Randomizer.GetRandomElement(Locale.Company.Value.BuzzNoun);
    }

    public virtual string Industry()
    {
        return Randomizer.GetRandomElement(Locale.Company.Value.Category);
    }

    public virtual string Suffix()
    {
        return Randomizer.GetRandomElement(Locale.Company.Value.LegalEntitySuffix);
    }
}