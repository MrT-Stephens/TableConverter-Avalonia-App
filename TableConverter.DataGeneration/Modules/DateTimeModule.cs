using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.Modules;

public enum DateTimeFormatsEnum
{
    ShortDate,
    LongDate,
    ShortTime,
    LongTime,
    FullDateTime,
    RFC1123,
    SortableDateTime,
    UniversalSortableDateTime,
    YearMonth,
    MonthDay
}

public class DateTimeModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : ModuleBase(faker, locale, randomizer)
{
    public override string ModuleName => "DateTime";

    public virtual string AnyTime(DateOnly referenceDate, TimeOnly referenceTime,
        DateTimeFormatsEnum format = DateTimeFormatsEnum.FullDateTime)
    {
        var dateTime = new DateTime(referenceDate, referenceTime);
        
        var minTicks = dateTime.Date.Ticks - 31536000000;
        var maxTicks = dateTime.Date.Ticks + 31536000000;
        
        if (minTicks < DateTime.MinValue.Ticks)
            minTicks = DateTime.MinValue.Ticks;
        
        if (maxTicks > DateTime.MaxValue.Ticks)
            maxTicks = DateTime.MaxValue.Ticks;

        var randomTicks = Randomizer.Long(minTicks, maxTicks);

        return FormatDateTime(new DateTime(randomTicks), format);
    }

    public virtual string Between(
        DateOnly fromDate = default,
        TimeOnly fromTime = default,
        DateOnly toDate = default,
        TimeOnly toTime = default,
        DateTimeFormatsEnum format = DateTimeFormatsEnum.FullDateTime)
    {
        var fromDateTime = new DateTime(fromDate, fromTime);
        var toDateTime = new DateTime(toDate, toTime);

        if (fromDateTime > toDateTime)
            throw new ArgumentException("From date should be less than To date", nameof(fromDate));

        var randomTicks = Randomizer.Long(fromDateTime.Ticks, toDateTime.Ticks);

        return FormatDateTime(new DateTime(randomTicks), format);
    }

    public virtual string Past(int years = 1000, DateTimeFormatsEnum format = DateTimeFormatsEnum.FullDateTime)
    {
        var randomTicks = Randomizer.Long(DateTime.Now.AddYears(-years).Ticks, DateTime.Now.Ticks);

        return FormatDateTime(new DateTime(randomTicks), format);
    }

    public virtual string Future(int years = 1000, DateTimeFormatsEnum format = DateTimeFormatsEnum.FullDateTime)
    {
        var randomTicks = Randomizer.Long(DateTime.Now.Ticks, DateTime.Now.AddYears(years).Ticks);

        return FormatDateTime(new DateTime(randomTicks), format);
    }

    public virtual string Recent(int days = 7, DateTimeFormatsEnum format = DateTimeFormatsEnum.FullDateTime)
    {
        var randomTicks = Randomizer.Long(DateTime.Now.AddDays(-days).Ticks, DateTime.Now.Ticks);

        return FormatDateTime(new DateTime(randomTicks), format);
    }

    public virtual string Soon(int days = 7, DateTimeFormatsEnum format = DateTimeFormatsEnum.FullDateTime)
    {
        var randomTicks = Randomizer.Long(DateTime.Now.Ticks, DateTime.Now.AddDays(days).Ticks);

        return FormatDateTime(new DateTime(randomTicks), format);
    }

    public virtual string BirthDate(int minAge = 18, int maxAge = 65,
        DateTimeFormatsEnum format = DateTimeFormatsEnum.FullDateTime)
    {
        var randomTicks = Randomizer.Long(DateTime.Now.AddYears(-maxAge).AddDays(-1).Ticks,
            DateTime.Now.AddYears(-minAge).Ticks);

        return FormatDateTime(new DateTime(randomTicks), format);
    }

    public virtual string Month(bool abbreviated = false)
    {
        return abbreviated
            ? Randomizer.GetRandomElement(Locale.Date.Value.Month.Abbreviated)
            : Randomizer.GetRandomElement(Locale.Date.Value.Month.Wide);
    }

    public virtual string Weekday(bool abbreviated = false)
    {
        return abbreviated
            ? Randomizer.GetRandomElement(Locale.Date.Value.Weekday.Abbreviated)
            : Randomizer.GetRandomElement(Locale.Date.Value.Weekday.Wide);
    }

    public virtual string TimeZone()
    {
        return Randomizer.GetRandomElement(Locale.Date.Value.TimeZone);
    }

    private static string FormatDateTime(DateTime dateTime, DateTimeFormatsEnum format)
    {
        return format switch
        {
            DateTimeFormatsEnum.ShortDate => dateTime.ToShortDateString(),
            DateTimeFormatsEnum.LongDate => dateTime.ToLongDateString(),
            DateTimeFormatsEnum.ShortTime => dateTime.ToShortTimeString(),
            DateTimeFormatsEnum.LongTime => dateTime.ToLongTimeString(),
            DateTimeFormatsEnum.FullDateTime => dateTime.ToString("F"),
            DateTimeFormatsEnum.RFC1123 => dateTime.ToString("r"),
            DateTimeFormatsEnum.SortableDateTime => dateTime.ToString("s"),
            DateTimeFormatsEnum.UniversalSortableDateTime => dateTime.ToUniversalTime().ToString("u"),
            DateTimeFormatsEnum.YearMonth => dateTime.ToString("Y"),
            DateTimeFormatsEnum.MonthDay => dateTime.ToString("M"),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }
}