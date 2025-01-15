using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.LocaleDataSets.en_GB;

/// <summary>
///     'en_GB' locale data set.
///     Used to generate data for the 'en_GB' locale.
///     This Locale uses the following data sets internally in decreasing order of priority:
///     <list type="bullet">
///         <item>
///             <description>en_GB (<see cref="en_GB.Locale" />).</description>
///         </item>
///         <item>
///             <description>en (<see cref="en.Locale" />).</description>
///         </item>
///         <item>
///             <description>base Locales (<see cref="LocaleBase" />).</description>
///         </item>
///     </list>
/// </summary>
[Locale("en_GB")]
public sealed class Locale : en.Locale
{
    public override string Title => "English (United Kingdom)";
    
    public override string Code => "en_GB";

    public override Lazy<PersonBase> Person { get; } = new(() => new PersonDataSet());

    public override Lazy<LocationBase> Location { get; } = new(() => new LocationDataSet());

    public override Lazy<PhoneNumberBase> PhoneNumber { get; } = new(() => new PhoneNumberDataSet());

    public override Lazy<InternetBase> Internet { get; } = new(() => new InternetDataSet());
}