using TableConverter.DataGeneration;
using TableConverter.DataGeneration.LocaleDataSetsBase;
using TableConverter.DataGeneration.Modules;
using TableConverter.DataModels;

namespace TableConverter.Services.DataGenerationAttributedModules;

[DataGenerationModule("Company",
    "Module for generating company-related data, such as names, catchphrases, buzzwords, and industry terms.",
    "DataGenerationCompanyIcon")]
public class CompanyAttributedModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : CompanyModule(faker, locale, randomizer)
{
    [DataGenerationModuleMethod("Name",
        "Generates a random company name. Example: 'Zieme, Hauck and McClure'")]
    public override string Name()
    {
        return base.Name();
    }

    [DataGenerationModuleMethod("Catch Phrase",
        "Generates a random company catchphrase. Example: 'Upgradable systematic flexibility'")]
    public override string CatchPhrase()
    {
        return base.CatchPhrase();
    }

    [DataGenerationModuleMethod("Buzz Phrase",
        "Generates a random jargon-filled buzz phrase. Example: 'cultivate synergistic e-markets'")]
    public override string BuzzPhrase()
    {
        return base.BuzzPhrase();
    }

    [DataGenerationModuleMethod("Catch Phrase Adjective",
        "Generates a random adjective used in company catchphrases. Example: 'Multi-tiered'")]
    public override string CatchPhraseAdjective()
    {
        return base.CatchPhraseAdjective();
    }

    [DataGenerationModuleMethod("Catch Phrase Descriptor",
        "Generates a random descriptor used in company catchphrases. Example: 'composite'")]
    public override string CatchPhraseDescriptor()
    {
        return base.CatchPhraseDescriptor();
    }

    [DataGenerationModuleMethod("Catch Phrase Noun",
        "Generates a random noun used in company catchphrases. Example: 'leverage'")]
    public override string CatchPhraseNoun()
    {
        return base.CatchPhraseNoun();
    }

    [DataGenerationModuleMethod("Buzz Adjective",
        "Generates a random adjective commonly used in business jargon. Example: 'one-to-one'")]
    public override string BuzzAdjective()
    {
        return base.BuzzAdjective();
    }

    [DataGenerationModuleMethod("Buzz Verb",
        "Generates a random verb commonly used in business jargon. Example: 'empower'")]
    public override string BuzzVerb()
    {
        return base.BuzzVerb();
    }

    [DataGenerationModuleMethod("Buzz Noun",
        "Generates a random noun commonly used in business jargon. Example: 'paradigms'")]
    public override string BuzzNoun()
    {
        return base.BuzzNoun();
    }

    [DataGenerationModuleMethod("Industry",
        "Generates a random industry name. Example: 'Information Technology' or 'Healthcare'")]
    public override string Industry()
    {
        return base.Industry();
    }

    [DataGenerationModuleMethod("Suffix",
        "Generates a random company suffix. Example: 'LLC', 'Inc.', or 'Ltd.'")]
    public override string Suffix()
    {
        return base.Suffix();
    }
}