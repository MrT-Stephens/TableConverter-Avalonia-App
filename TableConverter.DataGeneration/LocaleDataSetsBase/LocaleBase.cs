namespace TableConverter.DataGeneration.LocaleDataSetsBase;

/// <summary>
/// Base class for locale datasets.
/// Contains a title and multiple datasets.
/// </summary>
public abstract class LocaleBase
{
    public abstract string Title { get; }
    
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
}