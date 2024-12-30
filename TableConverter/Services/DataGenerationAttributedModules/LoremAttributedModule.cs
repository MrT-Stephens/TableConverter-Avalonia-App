using TableConverter.DataGeneration;
using TableConverter.DataGeneration.LocaleDataSetsBase;
using TableConverter.DataGeneration.Modules;
using TableConverter.DataModels;

namespace TableConverter.Services.DataGenerationAttributedModules;

[DataGenerationModule("Lorem", 
    "Module for generating random text such as words, sentences, and paragraphs.",
    "DataGenerationLoremIcon")]
public class LoremAttributedModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : LoremModule(faker, locale, randomizer)
{
    [DataGenerationModuleMethod("Word",
        "Generates a random word, optionally constrained by minimum and maximum length.")]
    public override string Word(int minLength = 1, int maxLength = int.MaxValue)
    {
        return base.Word(minLength, maxLength);
    }

    [DataGenerationModuleMethod("Words",
        "Generates a sequence of random words, constrained by minimum and maximum word count.")]
    public override string Words(int minCount = 3, int maxCount = 10)
    {
        return base.Words(minCount, maxCount);
    }

    [DataGenerationModuleMethod("Sentence",
        "Generates a random sentence, optionally constrained by minimum and maximum word count.")]
    public override string Sentence(int minCount = 3, int maxCount = 10)
    {
        return base.Sentence(minCount, maxCount);
    }

    [DataGenerationModuleMethod("Sentences",
        "Generates a series of random sentences, constrained by minimum and maximum sentence count and a separator between sentences.")]
    public override string Sentences(int minCount = 3, int maxCount = 10, string separator = " ")
    {
        return base.Sentences(minCount, maxCount, separator);
    }

    [DataGenerationModuleMethod("Paragraph",
        "Generates a random paragraph, optionally constrained by minimum and maximum sentence count.")]
    public override string Paragraph(int minCount = 3, int maxCount = 10)
    {
        return base.Paragraph(minCount, maxCount);
    }

    [DataGenerationModuleMethod("Paragraphs",
        "Generates multiple random paragraphs, constrained by minimum and maximum paragraph count and a separator between paragraphs.")]
    public override string Paragraphs(int minCount = 3, int maxCount = 6, string separator = "\n")
    {
        return base.Paragraphs(minCount, maxCount, separator);
    }

    [DataGenerationModuleMethod("Lines",
        "Generates a random number of lines of text, constrained by minimum and maximum line count.")]
    public override string Lines(int minCount = 3, int maxCount = 6)
    {
        return base.Lines(minCount, maxCount);
    }

    [DataGenerationModuleMethod("Text",
        "Generates a block of random text by randomly selecting any of the text generation functions (e.g., Word, Sentence, Paragraph).")]
    public override string Text()
    {
        return base.Text();
    }
}