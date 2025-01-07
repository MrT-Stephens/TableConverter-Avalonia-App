using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.LocaleDataSets.zh_CN;

/// <summary>
/// 'zh_CN' locale data set.
/// Used to generate data for the 'zh_CN' locale.
/// This Locale uses the following data sets internally in decreasing order of priority:
/// <list type="bullet">
///     <item>
///         <description>zh_CN (<see cref="zh_CN.Locale"/>).</description>
///     </item>
///     <item>
///         <description>en (<see cref="en.Locale"/>).</description>
///     </item>
///     <item>
///         <description>base Locales (<see cref="LocaleBase"/>).</description>
///     </item>
/// </list>
/// </summary>
[Locale("zh_CN")]
public sealed class Locale : en.Locale
{
    public override string Title => "Chinese (China)";
    
    public override Lazy<PersonBase> Person { get; } = new(() => new PersonDataSet());
    
    public override Lazy<PhoneNumberBase> PhoneNumber { get; } = new(() => new PhoneNumberDataSet());

    public override Lazy<LocationBase> Location { get; } = new(() => new LocationDataSet());
    
    public override Lazy<InternetBase> Internet { get; } = new(() => new InternetDataSet());

    public override Lazy<WordBase> Word { get; } = new(() => new WordDataSet());
    
    public override Lazy<ScienceBase> Science { get; } = new(() => new ScienceDataSet());

    public override Lazy<MusicBase> Music { get; } = new(() => new MusicDataSet());
    
    public override Lazy<ColorBase> Color { get; } = new(() => new ColorDataSet());

    public override Lazy<VehicleBase> Vehicle { get; } = new(() => new VehicleDataSet());
    
    public override Lazy<CommerceBase> Commerce { get; } = new(() => new CommerceDataSet());

    public override Lazy<DateBase> Date { get; } = new(() => new DateDataSet());
}