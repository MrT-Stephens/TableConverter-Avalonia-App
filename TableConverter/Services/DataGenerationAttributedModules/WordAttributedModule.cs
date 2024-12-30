using TableConverter.DataGeneration;
using TableConverter.DataGeneration.LocaleDataSetsBase;
using TableConverter.DataGeneration.Modules;
using TableConverter.DataModels;

namespace TableConverter.Services.DataGenerationAttributedModules;

[DataGenerationModule("Word",
    "Module for generating random words and word-related data such as nouns, verbs, and phrases.",
    "DataGenerationWordIcon")]
public class WordAttributedModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : WordModule(faker, locale, randomizer)
{
    [DataGenerationModuleMethod("Adjective",
        "Generates a random adjective, optionally constrained by minimum and maximum length. Example: 'beautiful'")]
    public override string Adjective(int minLength = 0, int maxLength = int.MaxValue)
    {
        return base.Adjective(minLength, maxLength);
    }

    [DataGenerationModuleMethod("Adverb",
        "Generates a random adverb, optionally constrained by minimum and maximum length. Example: 'quickly'")]
    public override string Adverb(int minLength = 0, int maxLength = int.MaxValue)
    {
        return base.Adverb(minLength, maxLength);
    }

    [DataGenerationModuleMethod("Conjunction",
        "Generates a random conjunction, optionally constrained by minimum and maximum length. Example: 'and'")]
    public override string Conjunction(int minLength = 0, int maxLength = int.MaxValue)
    {
        return base.Conjunction(minLength, maxLength);
    }

    [DataGenerationModuleMethod("Interjection",
        "Generates a random interjection, optionally constrained by minimum and maximum length. Example: 'wow'")]
    public override string Interjection(int minLength = 0, int maxLength = int.MaxValue)
    {
        return base.Interjection(minLength, maxLength);
    }

    [DataGenerationModuleMethod("Noun",
        "Generates a random noun, optionally constrained by minimum and maximum length. Example: 'table'")]
    public override string Noun(int minLength = 0, int maxLength = int.MaxValue)
    {
        return base.Noun(minLength, maxLength);
    }

    [DataGenerationModuleMethod("Preposition",
        "Generates a random preposition, optionally constrained by minimum and maximum length. Example: 'under'")]
    public override string Preposition(int minLength = 0, int maxLength = int.MaxValue)
    {
        return base.Preposition(minLength, maxLength);
    }

    [DataGenerationModuleMethod("Verb",
        "Generates a random verb, optionally constrained by minimum and maximum length. Example: 'run'")]
    public override string Verb(int minLength = 0, int maxLength = int.MaxValue)
    {
        return base.Verb(minLength, maxLength);
    }

    [DataGenerationModuleMethod("Sample",
        "Generates a random word from any of the categories (adjective, noun, verb, etc.), optionally constrained by minimum and maximum length. Example: 'run' (verb) or 'quickly' (adverb).")]
    public override string Sample(int minLength = 0, int maxLength = int.MaxValue)
    {
        return base.Sample(minLength, maxLength);
    }

    [DataGenerationModuleMethod("Words",
        "Generates a series of random words, constrained by the minimum and maximum count. Example: 'apple banana cherry'")]
    public override string Words(int minCount = 3, int maxCount = 10)
    {
        return base.Words(minCount, maxCount);
    }
}