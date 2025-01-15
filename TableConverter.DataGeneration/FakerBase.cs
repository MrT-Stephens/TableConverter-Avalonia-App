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
    /// <summary>
    ///     Cached locales. Used to store locales that have already been created.
    ///     This is to prevent creating the same locale multiple times.
    /// </summary>
    private readonly Dictionary<string, ILocale> _CachedLocales = new();

    /// <summary>
    ///     The internal locale name of the dataset. E.g. "en".
    /// </summary>
    private string _LocaleType = null!;

    protected FakerBase(string localeType = "en", int? seed = null)
    {
        LocaleType = localeType;
        Randomizer = seed.HasValue ? new Randomizer(seed.Value) : new Randomizer();
    }

    /// <summary>
    ///     The locale type of the dataset. E.g. "en".
    /// </summary>
    public string LocaleType
    {
        get => _LocaleType;
        set
        {
            if (_LocaleType == value) return;

            _LocaleType = value;

            if (_CachedLocales.TryGetValue(_LocaleType, out var locale))
            {
                Locale = (LocaleBase)locale;
            }
            else
            {
                Locale = (LocaleBase)LocaleFactory.CreateLocale(_LocaleType);
                _CachedLocales.Add(_LocaleType, Locale);
            }
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
    
    public LocaleBase Locale { get; set; } = null!;

    public Randomizer Randomizer { get; set; }

    public void Seed(int seed)
    {
        Randomizer = new Randomizer(seed);
    }
}