using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.Modules;

public class WordModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : ModuleBase(faker, locale, randomizer)
{
    public override string ModuleName => "Word";

    public virtual string Adjective(int minLength = 0, int maxLength = int.MaxValue)
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

        var data = FilterStringList(Locale.Word.Value.Adjective, minLength, maxLength);

        return data.Count == 0 ? string.Empty : Randomizer.GetRandomElement(data);
    }

    public virtual string Adverb(int minLength = 0, int maxLength = int.MaxValue)
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

        var data = FilterStringList(Locale.Word.Value.Adverb, minLength, maxLength);

        return data.Count == 0 ? string.Empty : Randomizer.GetRandomElement(data);
    }

    public virtual string Conjunction(int minLength = 0, int maxLength = int.MaxValue)
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

        var data = FilterStringList(Locale.Word.Value.Conjunction, minLength, maxLength);

        return data.Count == 0 ? string.Empty : Randomizer.GetRandomElement(data);
    }

    public virtual string Interjection(int minLength = 0, int maxLength = int.MaxValue)
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

        var data = FilterStringList(Locale.Word.Value.Interjection, minLength, maxLength);

        return data.Count == 0 ? string.Empty : Randomizer.GetRandomElement(data);
    }

    public virtual string Noun(int minLength = 0, int maxLength = int.MaxValue)
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

        var data = FilterStringList(Locale.Word.Value.Noun, minLength, maxLength);

        return data.Count == 0 ? string.Empty : Randomizer.GetRandomElement(data);
    }

    public virtual string Preposition(int minLength = 0, int maxLength = int.MaxValue)
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

        var data = FilterStringList(Locale.Word.Value.Preposition, minLength, maxLength);

        return data.Count == 0 ? string.Empty : Randomizer.GetRandomElement(data);
    }

    public virtual string Verb(int minLength = 0, int maxLength = int.MaxValue)
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

        var data = FilterStringList(Locale.Word.Value.Verb, minLength, maxLength);

        return data.Count == 0 ? string.Empty : Randomizer.GetRandomElement(data);
    }

    public virtual string Sample(int minLength = 0, int maxLength = int.MaxValue)
    {
        var number = Randomizer.Number(0, 6);

        return number switch
        {
            0 => Adjective(minLength, maxLength),
            1 => Adverb(minLength, maxLength),
            2 => Conjunction(minLength, maxLength),
            3 => Interjection(minLength, maxLength),
            4 => Noun(minLength, maxLength),
            5 => Preposition(minLength, maxLength),
            6 => Verb(minLength, maxLength),
            _ => throw new ArgumentOutOfRangeException(nameof(number), number, "Number must be between 0 and 6")
        };
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

        return string.Join(" ", Enumerable.Range(0, count).Select(_ => Sample()));
    }

    private static IReadOnlyList<string> FilterStringList(IReadOnlyList<string> list, int minLength, int maxLength)
    {
        return list.Where(word => word.Length >= minLength && word.Length <= maxLength).ToList();
    }
}