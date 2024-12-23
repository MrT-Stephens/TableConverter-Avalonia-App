using System.Collections.Immutable;
using TableConverter.DataGeneration.DataModels;
using TableConverter.DataGeneration.Interfaces;
using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.LocaleDataSets.en;

public class PhoneNumberDataSet : PhoneNumberBase
{
    public override ImmutableArray<IWeightedValue<string>> HumanPattern { get; } =
    [
        new WeightedValue<string>("!##-!##-####", 1),
        new WeightedValue<string>("(!##) !##-####", 1),
        new WeightedValue<string>("1-!##-!##-####", 1),
        new WeightedValue<string>("!##.!##.####", 1),
        new WeightedValue<string>("!##-!##-#### x###", 1),
        new WeightedValue<string>("(!##) !##-#### x###", 1),
        new WeightedValue<string>("1-!##-!##-#### x###", 1),
        new WeightedValue<string>("!##.!##.#### x###", 1),
        new WeightedValue<string>("!##-!##-#### x####", 1),
        new WeightedValue<string>("(!##) !##-#### x####", 1),
        new WeightedValue<string>("1-!##-!##-#### x####", 1),
        new WeightedValue<string>("!##.!##.#### x####", 1),
        new WeightedValue<string>("!##-!##-#### x#####", 1),
        new WeightedValue<string>("(!##) !##-#### x#####", 1),
        new WeightedValue<string>("1-!##-!##-#### x#####", 1),
        new WeightedValue<string>("!##.!##.#### x#####", 1)
    ];

    public override ImmutableArray<IWeightedValue<string>> InternationalPattern { get; } =
    [
        new WeightedValue<string>("+1!##!######", 1)
    ];

    public override ImmutableArray<IWeightedValue<string>> NationalPattern { get; } =
    [
        new WeightedValue<string>("(!##) !##-####", 1)
    ];
}