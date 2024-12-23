using System.Collections.Immutable;
using TableConverter.DataGeneration.DataModels;
using TableConverter.DataGeneration.Interfaces;
using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.LocaleDataSets.en;

public class InternetDataSet : InternetBase
{
    public override ImmutableArray<IWeightedValue<string>> DomainSuffix { get; } =
    [
        new WeightedValue<string>("com", 30),
        new WeightedValue<string>("net", 30),
        new WeightedValue<string>("org", 20),
        new WeightedValue<string>("info", 10),
        new WeightedValue<string>("biz", 10),
        new WeightedValue<string>("name", 10)
    ];

    public override ImmutableArray<IWeightedValue<string>> ExampleEmail { get; } =
    [
        new WeightedValue<string>("example.com", 30),
        new WeightedValue<string>("example.net", 30),
        new WeightedValue<string>("example.org", 20)
    ];

    public override ImmutableArray<IWeightedValue<string>> FreeEmail { get; } =
    [
        new WeightedValue<string>("gmail.com", 50),
        new WeightedValue<string>("yahoo.com", 30),
        new WeightedValue<string>("hotmail.com", 30)
    ];
}