using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.LocaleDataSets.en_GB;

[Locale("en_GB")]
public class Locale : en.Locale
{
    public override string Title => "English (United Kingdom)";

    public override Lazy<PersonBase> Person { get; } = new(() => new PersonDataSet());

    public override Lazy<LocationBase> Location { get; } = new(() => new LocationDataSet());

    public override Lazy<PhoneNumberBase> PhoneNumber { get; } = new(() => new PhoneNumberDataSet());
    
    public override Lazy<InternetBase> Internet { get; } = new(() => new InternetDataSet());
}