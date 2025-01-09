using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.LocaleDataSets.en;

/// <summary>
///     'en' locale data set.
///     Used to generate data for the 'en' locale.
///     This Locale uses the following data sets internally in decreasing order of priority:
///     <list type="bullet">
///         <item>
///             <description>en (<see cref="en.Locale" />).</description>
///         </item>
///         <item>
///             <description>base Locales (<see cref="LocaleBase" />).</description>
///         </item>
///     </list>
/// </summary>
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

    public override Lazy<VehicleBase> Vehicle { get; } = new(() => new VehicleDataSet());

    public override Lazy<CompanyBase> Company { get; } = new(() => new CompanyDataSet());

    public override Lazy<CommerceBase> Commerce { get; } = new(() => new CommerceDataSet());

    public override Lazy<FoodBase> Food { get; } = new(() => new FoodDataSet());

    public override Lazy<DateBase> Date { get; } = new(() => new DateDataSet());
}