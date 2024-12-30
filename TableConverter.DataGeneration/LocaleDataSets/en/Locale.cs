using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.LocaleDataSets.en;

[Locale("en")]
public class Locale : LocaleBase
{
    public override string Title => "English";

    public override Lazy<PersonBase> Person { get; } = new(() => new PersonDataSet());
    
    public override Lazy<PhoneNumberBase> PhoneNumber { get; } = new(() => new PhoneNumberDataSet());
    
    public override Lazy<LocationBase> Location { get; } = new(() => new LocationDataSet());

    public override Lazy<InternetBase> Internet { get; } = new(() => new InternetDataSet());
    
    public override Lazy<WordBase> Word { get; } = new(() => new WordDataSet());
    
    public override Lazy<LoremBase> Lorem { get; } = new(() => new LoremDataSet());
    
    public override Lazy<SystemBase> System { get; } = new(() => new SystemDataSet());
    
    public override Lazy<ScienceBase> Science { get; } = new(() => new ScienceDataSet());
    
    public override Lazy<MusicBase> Music { get; } = new(() => new MusicDataSet());
    
    public override Lazy<ColorBase> Color { get; } = new(() => new ColorDataSet());
}