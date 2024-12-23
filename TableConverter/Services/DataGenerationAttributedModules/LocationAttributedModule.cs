using TableConverter.DataGeneration;
using TableConverter.DataGeneration.LocaleDataSetsBase;
using TableConverter.DataGeneration.Modules;
using TableConverter.DataModels;

namespace TableConverter.Services.DataGenerationAttributedModules;

[DataGenerationModule("Location",
    "Module for generating location-related data such as addresses, coordinates, and geographical names.")]
public class LocationAttributedModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : LocationModule(faker, locale, randomizer)
{
    [DataGenerationModuleMethod("Cardinal Direction",
        "Generates a cardinal direction (North, East, South, West). Example: 'North' or 'N'")]
    public override string CardinalDirection(bool abbreviated = false)
    {
        return base.CardinalDirection(abbreviated);
    }

    [DataGenerationModuleMethod("Ordinal Direction",
        "Generates an ordinal direction (e.g., Northeast, Southwest). Example: 'Northeast' or 'NE'")]
    public override string OrdinalDirection(bool abbreviated = false)
    {
        return base.OrdinalDirection(abbreviated);
    }

    [DataGenerationModuleMethod("Building Number", "Generates a random building number. Example: '123' or '12A'")]
    public override string BuildingNumber()
    {
        return base.BuildingNumber();
    }

    [DataGenerationModuleMethod("City", "Generates a random city name. Example: 'San Francisco'")]
    public override string City()
    {
        return base.City();
    }

    [DataGenerationModuleMethod("Postal Code", "Generates a random postal code. Example: '94107' or '90210'")]
    public override string PostalCode()
    {
        return base.PostalCode();
    }

    [DataGenerationModuleMethod("Continent", "Generates a random continent name. Example: 'North America' or 'Asia'")]
    public override string Continent()
    {
        return base.Continent();
    }

    [DataGenerationModuleMethod("Country", "Generates a random country name. Example: 'United States' or 'Germany'")]
    public override string Country()
    {
        return base.Country();
    }

    [DataGenerationModuleMethod("Country Code",
        "Generates a country code in the specified format (Alpha-2 or Alpha-3). Example: 'US' or 'USA'")]
    public override string CountryCode(CountryCodeFormatEnum format = CountryCodeFormatEnum.Alpha2)
    {
        return base.CountryCode(format);
    }

    [DataGenerationModuleMethod("Direction",
        "Generates a compass direction (e.g., North, Northeast). Example: 'North' or 'N'")]
    public override string Direction(bool abbreviated = false)
    {
        return base.Direction(abbreviated);
    }

    [DataGenerationModuleMethod("Latitude", "Generates a random latitude coordinate. Example: '37.7749'")]
    public override string Latitude(int min = -90, int max = 90, int precision = 4)
    {
        return base.Latitude(min, max, precision);
    }

    [DataGenerationModuleMethod("Longitude", "Generates a random longitude coordinate. Example: '-122.4194'")]
    public override string Longitude(int min = -180, int max = 180, int precision = 4)
    {
        return base.Longitude(min, max, precision);
    }

    [DataGenerationModuleMethod("Secondary Address",
        "Generates a secondary address (e.g., apartment or suite number). Example: 'Apt. 4B'")]
    public override string SecondaryAddress()
    {
        return base.SecondaryAddress();
    }

    [DataGenerationModuleMethod("State", "Generates a random state or province name. Example: 'California' or 'CA'")]
    public override string State(bool abbreviated = false)
    {
        return base.State(abbreviated);
    }

    [DataGenerationModuleMethod("Street", "Generates a random street name. Example: 'Market Street'")]
    public override string Street()
    {
        return base.Street();
    }

    [DataGenerationModuleMethod("Street Address",
        "Generates a random street address. Example: '123 Market Street' or '123 Market Street, Apt. 4B'")]
    public override string StreetAddress(bool useFullAddress = true)
    {
        return base.StreetAddress(useFullAddress);
    }

    [DataGenerationModuleMethod("Time Zone", "Generates a random time zone. Example: 'America/Los_Angeles'")]
    public override string TimeZone()
    {
        return base.TimeZone();
    }

    [DataGenerationModuleMethod("County", "Generates a random county name. Example: 'Santa Clara County'")]
    public override string County()
    {
        return base.County();
    }

    [DataGenerationModuleMethod("Language",
        "Generates a random language name in the specified format. Example: 'English' or 'en'")]
    public override string Language(LanguageFormatEnum format = LanguageFormatEnum.Normal)
    {
        return base.Language(format);
    }
}