using System.Globalization;
using TableConverter.DataGeneration.Exceptions;
using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.Modules;

public enum CountryCodeFormatEnum
{
    Alpha2,
    Alpha3,
    Numeric
}

public enum LanguageFormatEnum
{
    Normal,
    Alpha2,
    Alpha3
}

public class LocationModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : ModuleBase(faker, locale, randomizer)
{
    public override string ModuleName => "Location";

    public virtual string CardinalDirection(bool abbreviated = false)
    {
        return abbreviated
            ? Randomizer.GetRandomElement(Locale.Location.Value.Direction.CardinalAbbr)
            : Randomizer.GetRandomElement(Locale.Location.Value.Direction.Cardinal);
    }

    public virtual string OrdinalDirection(bool abbreviated = false)
    {
        return abbreviated
            ? Randomizer.GetRandomElement(Locale.Location.Value.Direction.OrdinalAbbr)
            : Randomizer.GetRandomElement(Locale.Location.Value.Direction.Ordinal);
    }

    public virtual string BuildingNumber()
    {
        return Randomizer.ReplaceSymbolsWithNumbers(
            Randomizer.GetWeightedValue(Locale.Location.Value.BuildingNumberPattern));
    }

    public virtual string City()
    {
        return Randomizer.GetWeightedValue(Locale.Location.Value.CityPattern).Build(Faker, Locale, Randomizer);
    }

    public virtual string PostalCode()
    {
        return Randomizer.ReplaceSymbols(Randomizer.GetWeightedValue(Locale.Location.Value.PostCodePattern));
    }

    public virtual string Continent()
    {
        return Randomizer.GetRandomElement(Locale.Location.Value.Continent);
    }

    public virtual string Country()
    {
        return Randomizer.GetRandomElement(Locale.Location.Value.Country);
    }

    public virtual string CountryCode(CountryCodeFormatEnum format = CountryCodeFormatEnum.Alpha2)
    {
        var val = Randomizer.GetRandomElement(Locale.Location.Value.CountryCode);

        return format switch
        {
            CountryCodeFormatEnum.Alpha2 => val.Alpha2,
            CountryCodeFormatEnum.Alpha3 => val.Alpha3,
            CountryCodeFormatEnum.Numeric => val.Numeric,
            _ => FakerArgumentException.CreateException<string>(format, "Invalid country code")
        };
    }

    public virtual string Direction(bool abbreviated = false)
    {
        return Randomizer.Bool()
            ? abbreviated
                ? Randomizer.GetRandomElement(Locale.Location.Value.Direction.CardinalAbbr)
                : Randomizer.GetRandomElement(Locale.Location.Value.Direction.Cardinal)
            : abbreviated
                ? Randomizer.GetRandomElement(Locale.Location.Value.Direction.OrdinalAbbr)
                : Randomizer.GetRandomElement(Locale.Location.Value.Direction.Ordinal);
    }

    public virtual string Latitude(int min = -90, int max = 90, int precision = 4)
    {
        return Randomizer.Float(min, max).ToString($"F{precision}", CultureInfo.InvariantCulture);
    }

    public virtual string Longitude(int min = -180, int max = 180, int precision = 4)
    {
        return Randomizer.Float(min, max).ToString($"F{precision}", CultureInfo.InvariantCulture);
    }

    public virtual string SecondaryAddress()
    {
        return Randomizer.ReplaceSymbolsWithNumbers(
            Randomizer.GetWeightedValue(Locale.Location.Value.SecondaryAddressPattern));
    }

    public virtual string State(bool abbreviated = false)
    {
        return abbreviated
            ? Randomizer.GetRandomElement(Locale.Location.Value.StateAbbr)
            : Randomizer.GetRandomElement(Locale.Location.Value.State);
    }

    public virtual string Street()
    {
        return Randomizer.GetWeightedValue(Locale.Location.Value.StreetPattern).Build(Faker, Locale, Randomizer);
    }

    public virtual string StreetAddress(bool useFullAddress = true)
    {
        return useFullAddress
            ? Randomizer.GetWeightedValue(Locale.Location.Value.StreetAddress.Full).Build(Faker, Locale, Randomizer)
            : Randomizer.GetWeightedValue(Locale.Location.Value.StreetAddress.Normal).Build(Faker, Locale, Randomizer);
    }

    public virtual string TimeZone()
    {
        return Randomizer.GetRandomElement(Locale.Location.Value.TimeZone);
    }

    public virtual string County()
    {
        return Randomizer.GetRandomElement(Locale.Location.Value.County);
    }

    public virtual string Language(LanguageFormatEnum format = LanguageFormatEnum.Normal)
    {
        var val = Randomizer.GetRandomElement(Locale.Location.Value.Language);

        return format switch
        {
            LanguageFormatEnum.Normal => val.Name,
            LanguageFormatEnum.Alpha2 => val.Alpha2,
            LanguageFormatEnum.Alpha3 => val.Alpha3,
            _ => FakerArgumentException.CreateException<string>(format, "Invalid language")
        };
    }
}