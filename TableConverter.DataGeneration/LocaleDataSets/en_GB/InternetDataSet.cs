using System.Collections.Immutable;
using TableConverter.DataGeneration.DataModels;
using TableConverter.DataGeneration.Interfaces;

namespace TableConverter.DataGeneration.LocaleDataSets.en_GB;

public class InternetDataSet : en.InternetDataSet
{
    public override ImmutableArray<IWeightedValue<string>> DomainSuffix { get; } =
    [
        new WeightedValue<string>("ac.uk", 50),
        new WeightedValue<string>("biz", 10),
        new WeightedValue<string>("co", 10),
        new WeightedValue<string>("co.uk", 50),
        new WeightedValue<string>("com", 60),
        new WeightedValue<string>("cymru", 30),
        new WeightedValue<string>("gov.uk", 40),
        new WeightedValue<string>("info", 20),
        new WeightedValue<string>("london", 10),
        new WeightedValue<string>("ltd.uk", 20),
        new WeightedValue<string>("me.uk", 10),
        new WeightedValue<string>("name", 10),
        new WeightedValue<string>("nhs.uk", 30),
        new WeightedValue<string>("org.uk", 30),
        new WeightedValue<string>("plc.uk", 20),
        new WeightedValue<string>("sch.uk", 10),
        new WeightedValue<string>("scot", 5),
        new WeightedValue<string>("uk", 50),
        new WeightedValue<string>("wales", 30)
    ];
}