using TableConverter.DataGeneration.Interfaces;
using TableConverter.DataGeneration.LocaleDataSetsBase;
using TableConverter.DataGeneration.Modules;

namespace TableConverter.DataGeneration;

/// <summary>
///     Base class for all Faker classes.
///     Contain the locale dataset and the locale type.
/// </summary>
public abstract class FakerBase : IFaker
{
    private string _localeType = null!;

    public LocaleBase Locale { get; set; } = null!;

    public Randomizer Randomizer { get; set; }

    protected FakerBase(string localeType = "en", int? seed = null)
    {
        LocaleType = localeType;
        Randomizer = seed.HasValue ? new Randomizer(seed.Value) : new Randomizer();
    }

    public string LocaleType
    {
        get => _localeType;
        set
        {
            if (_localeType == value) return;

            _localeType = value;
            Locale = LocaleFactory.CreateLocale(_localeType);
        }
    }

    public abstract PersonModule Person { get; }
    public abstract PhoneModule Phone { get; }
    public abstract LocationModule Location { get; }
    public abstract InternetModule Internet { get; }
    public abstract WordModule Word { get; }
    public abstract LoremModule Lorem { get; }
    public abstract SystemModule System { get; }
    public abstract ScienceModule Science { get; }
    public abstract MusicModule Music { get; }
    public abstract NumberModule Number { get; }
    public abstract ImageModule Image { get; }
    public abstract ColorModule Color { get; }
    public abstract VehicleModule Vehicle { get; }
    public abstract CompanyModule Company { get; }
    public abstract CommerceModule Commerce { get; }
    public abstract FoodModule Food { get; }
    public abstract DateTimeModule DateTime { get; }

    public void Seed(int seed)
    {
        Randomizer = new Randomizer(seed);
    }
}