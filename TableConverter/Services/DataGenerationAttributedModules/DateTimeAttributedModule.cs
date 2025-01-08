using System;
using TableConverter.DataGeneration;
using TableConverter.DataGeneration.LocaleDataSetsBase;
using TableConverter.DataGeneration.Modules;
using TableConverter.DataModels;

namespace TableConverter.Services.DataGenerationAttributedModules;

[DataGenerationModule("DateTime", "Generates dates and times with various options for past, future, or random values.",
    "DataGenerationDateTimeIcon")]
public class DateTimeAttributedModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : DateTimeModule(faker, locale, randomizer)
{
    [DataGenerationModuleMethod("Between", "Generate a random date between two specified dates.")]
    public override string Between(
        DateOnly fromDate = default,
        TimeOnly fromTime = default,
        DateOnly toDate = default,
        TimeOnly toTime = default,
        DateTimeFormatsEnum format = DateTimeFormatsEnum.FullDateTime)
    {
        return base.Between(fromDate, fromTime, toDate, toTime, format);
    }

    [DataGenerationModuleMethod("Future", "Generate a random date in the future within a specified number of years.")]
    public override string Future(int years = 1000, DateTimeFormatsEnum format = DateTimeFormatsEnum.FullDateTime)
    {
        return base.Future(years, format);
    }

    [DataGenerationModuleMethod("Past", "Generate a random date in the past within a specified number of years.")]
    public override string Past(int years = 1000, DateTimeFormatsEnum format = DateTimeFormatsEnum.FullDateTime)
    {
        return base.Past(years, format);
    }

    [DataGenerationModuleMethod("Recent", "Generate a random date within the past specified number of days.")]
    public override string Recent(int days = 7, DateTimeFormatsEnum format = DateTimeFormatsEnum.FullDateTime)
    {
        return base.Recent(days, format);
    }

    [DataGenerationModuleMethod("Soon", "Generate a random date within the next specified number of days.")]
    public override string Soon(int days = 7, DateTimeFormatsEnum format = DateTimeFormatsEnum.FullDateTime)
    {
        return base.Soon(days, format);
    }

    [DataGenerationModuleMethod("Month", "Generate a random month name, optionally abbreviated.")]
    public override string Month(bool abbreviated = false)
    {
        return base.Month(abbreviated);
    }

    [DataGenerationModuleMethod("Weekday", "Generate a random day of the week, optionally abbreviated.")]
    public override string Weekday(bool abbreviated = false)
    {
        return base.Weekday(abbreviated);
    }

    [DataGenerationModuleMethod("Any Time", "Generate a random date and time relative to a specified reference date.")]
    public override string AnyTime(DateOnly referenceDate, TimeOnly referenceTime,
        DateTimeFormatsEnum format = DateTimeFormatsEnum.FullDateTime)
    {
        return base.AnyTime(referenceDate, referenceTime, format);
    }

    [DataGenerationModuleMethod("Birth Date", "Generate a random realistic birthdate within a specified age range.")]
    public override string BirthDate(int minAge = 18, int maxAge = 65,
        DateTimeFormatsEnum format = DateTimeFormatsEnum.FullDateTime)
    {
        return base.BirthDate(minAge, maxAge, format);
    }

    [DataGenerationModuleMethod("Time Zone", "Generate a random IANA time zone name.")]
    public override string TimeZone()
    {
        return base.TimeZone();
    }
}