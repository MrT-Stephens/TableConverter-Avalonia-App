using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.Modules;

public enum SexEnum
{
    Generic,
    Male,
    Female
}

public enum SexFormat
{
    UpperFullWord,
    LowerFullWord,
    UpperAbbreviation,
    LowerAbbreviation
}

public class PersonModule(FakerBase faker, LocaleBase locale, Randomizer randomizer) : ModuleBase(faker, locale, randomizer)
{
    public override string ModuleName => "Person";

    public virtual string FullName()
    {
        return Randomizer.GetWeightedValue(Locale.Person.Value.NamePattern).Build(Faker, Locale, Randomizer);
    }

    public virtual string FirstName(SexEnum sex)
    {
        return sex switch
        {
            SexEnum.Generic => Randomizer.GetRandomElement(Locale.Person.Value.FirstName.Generic),
            SexEnum.Male => Randomizer.GetRandomElement(Locale.Person.Value.FirstName.Male),
            SexEnum.Female => Randomizer.GetRandomElement(Locale.Person.Value.FirstName.Female),
            _ => throw new ArgumentOutOfRangeException(nameof(sex), sex, null)
        };
    }

    public virtual string MiddleName(SexEnum sex)
    {
        return sex switch
        {
            SexEnum.Generic => Randomizer.GetRandomElement(Locale.Person.Value.MiddleName.Generic),
            SexEnum.Male => Randomizer.GetRandomElement(Locale.Person.Value.MiddleName.Male),
            SexEnum.Female => Randomizer.GetRandomElement(Locale.Person.Value.MiddleName.Female),
            _ => throw new ArgumentOutOfRangeException(nameof(sex), sex, null)
        };
    }

    public virtual string LastName(SexEnum sex)
    {
        return sex switch
        {
            SexEnum.Generic => Randomizer.GetRandomElement(Locale.Person.Value.LastName.Generic),
            SexEnum.Male => Randomizer.GetRandomElement(Locale.Person.Value.LastName.Male),
            SexEnum.Female => Randomizer.GetRandomElement(Locale.Person.Value.LastName.Female),
            _ => throw new ArgumentOutOfRangeException(nameof(sex), sex, null)
        };
    }

    public virtual string Title(SexEnum sex)
    {
        return sex switch
        {
            SexEnum.Generic => Randomizer.GetRandomElement(Locale.Person.Value.Title.Generic),
            SexEnum.Male => Randomizer.GetRandomElement(Locale.Person.Value.Title.Male),
            SexEnum.Female => Randomizer.GetRandomElement(Locale.Person.Value.Title.Female),
            _ => throw new ArgumentOutOfRangeException(nameof(sex), sex, null)
        };
    }

    public virtual string Suffix(SexEnum sex)
    {
        return Randomizer.GetRandomElement(Locale.Person.Value.Suffix);
    }

    public virtual string Sex(SexFormat format)
    {
        var sex = SexType();

        return format switch
        {
            SexFormat.UpperFullWord => sex == SexEnum.Male ? "Male" : "Female",
            SexFormat.LowerFullWord => sex == SexEnum.Male ? "male" : "female",
            SexFormat.UpperAbbreviation => sex == SexEnum.Male ? "M" : "F",
            SexFormat.LowerAbbreviation => sex == SexEnum.Male ? "m" : "f",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }
    
    public virtual SexEnum SexType()
    {
        SexEnum[] sex = [SexEnum.Male, SexEnum.Female];

        return Randomizer.GetRandomElement(sex.ToList());
    }

    public virtual string Gender()
    {
        return Randomizer.GetRandomElement(Locale.Person.Value.Gender);
    }

    public virtual string Biography()
    {
        return Randomizer.GetWeightedValue(Locale.Person.Value.BiographyPattern).Build(Faker, Locale, Randomizer);
    }

    public virtual string JobType()
    {
        return Randomizer.GetWeightedValue(Locale.Person.Value.JobTypePattern).Build(Faker, Locale, Randomizer);
    }
}