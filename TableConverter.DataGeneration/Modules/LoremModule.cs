using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.Modules;

public class LoremModule(FakerBase faker, LocaleBase locale, Randomizer randomizer) : ModuleBase(faker, locale, randomizer)
{
    public override string ModuleName => "Lorem";

    public virtual string Word(int minLength = 1, int maxLength = int.MaxValue)
    {
        if (minLength < 0)
            throw new ArgumentOutOfRangeException(nameof(minLength), minLength,
                "minLength must be greater than or equal to 0");

        if (maxLength < 0)
            throw new ArgumentOutOfRangeException(nameof(maxLength), maxLength,
                "maxLength must be greater than or equal to 0");

        if (minLength > maxLength)
            throw new ArgumentOutOfRangeException(nameof(minLength), minLength,
                "minLength must be less than or equal to maxLength");

        var data = FilterStringList(Locale.Lorem.Value.Word, minLength, maxLength);

        return data.Count == 0 ? string.Empty : Randomizer.GetRandomElement(data);
    }

    public virtual string Words(int minCount = 3, int maxCount = 10)
    {
        if (minCount < 0)
            throw new ArgumentOutOfRangeException(nameof(minCount), minCount,
                "minCount must be greater than or equal to 0");

        if (maxCount < 0)
            throw new ArgumentOutOfRangeException(nameof(maxCount), maxCount,
                "maxCount must be greater than or equal to 0");

        if (minCount > maxCount)
            throw new ArgumentOutOfRangeException(nameof(minCount), minCount,
                "minCount must be less than or equal to maxCount");

        var count = Randomizer.Number(minCount, maxCount);

        return string.Join(" ", Enumerable.Range(0, count).Select(_ => Word()));
    }

    public virtual string Sentence(int minCount = 3, int maxCount = 10)
    {
        var words = Words(minCount, maxCount);

        return words.Length == 0
            ? string.Empty
            : $"{words[0].ToString().ToUpper()}{words[1..]}.";
    }

    public virtual string Sentences(int minCount = 3, int maxCount = 10, string separator = " ")
    {
        if (minCount < 0)
            throw new ArgumentOutOfRangeException(nameof(minCount), minCount,
                "minCount must be greater than or equal to 0");

        if (maxCount < 0)
            throw new ArgumentOutOfRangeException(nameof(maxCount), maxCount,
                "maxCount must be greater than or equal to 0");

        if (minCount > maxCount)
            throw new ArgumentOutOfRangeException(nameof(minCount), minCount,
                "minCount must be less than or equal to maxCount");

        var count = Randomizer.Number(minCount, maxCount);

        return string.Join(separator, Enumerable.Range(0, count).Select(_ => Sentence()));
    }
    
    public virtual string Paragraph(int minCount = 3, int maxCount = 10)
    {
        return Sentences(minCount, maxCount);
    }
    
    public virtual string Paragraphs(int minCount = 3, int maxCount = 6, string separator = "\n")
    {
        return Sentences(minCount, maxCount, separator);
    }
    
    public virtual string Lines(int minCount = 3, int maxCount = 6)
    {
        return Sentences(minCount, maxCount, Environment.NewLine);
    }

    public virtual string Text()
    {
        var number = Randomizer.Number(0, 6);
        
        return number switch
        {
            0 => Word(),
            1 => Words(),
            2 => Sentence(),
            3 => Sentences(),
            4 => Paragraph(),
            5 => Paragraphs(),
            6 => Lines(),
            _ => throw new ArgumentOutOfRangeException(nameof(number), number, "Number must be between 0 and 6")
        };
    }

    private static IReadOnlyList<string> FilterStringList(IReadOnlyList<string> list, int minLength, int maxLength)
    {
        return list.Where(word => word.Length >= minLength && word.Length <= maxLength).ToList();
    }
}