using TableConverter.DataGeneration.Interfaces;

namespace TableConverter.DataGeneration.LocaleDataSetsBase;

/// <summary>
///     Base class for locale datasets.
///     Contains a title and multiple datasets.
/// </summary>
public abstract class LocaleBase : ILocale
{
    public abstract string Title { get; }
    
    public abstract string Code { get; }

    public abstract Lazy<PersonBase> Person { get; }

    public abstract Lazy<PhoneNumberBase> PhoneNumber { get; }

    public abstract Lazy<LocationBase> Location { get; }

    public abstract Lazy<InternetBase> Internet { get; }

    public abstract Lazy<WordBase> Word { get; }

    public abstract Lazy<LoremBase> Lorem { get; }

    public abstract Lazy<SystemBase> System { get; }

    public abstract Lazy<ScienceBase> Science { get; }

    public abstract Lazy<MusicBase> Music { get; }

    public abstract Lazy<ColorBase> Color { get; }

    public abstract Lazy<VehicleBase> Vehicle { get; }

    public abstract Lazy<CompanyBase> Company { get; }

    public abstract Lazy<CommerceBase> Commerce { get; }

    public abstract Lazy<FoodBase> Food { get; }

    public abstract Lazy<DateBase> Date { get; }
}