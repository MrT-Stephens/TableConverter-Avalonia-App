using System.Collections.Immutable;
using TableConverter.DataGeneration.DataModels;
using TableConverter.DataGeneration.Interfaces;
using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.LocaleDataSets.zh_CN;

public class PhoneNumberDataSet : PhoneNumberBase
{
    public override ImmutableArray<IWeightedValue<string>> HumanPattern { get; } =
    [
        new WeightedValue<string>("0##-########", 1),
        new WeightedValue<string>("0###-########", 1),
        new WeightedValue<string>("1##########", 1)
    ];

    public override ImmutableArray<IWeightedValue<string>> InternationalPattern { get; } =
    [
        new WeightedValue<string>("+86##########", 1),
        new WeightedValue<string>("+86###########", 1),
        new WeightedValue<string>("+861##########", 1)
    ];

    public override ImmutableArray<IWeightedValue<string>> NationalPattern { get; } =
    [
        new WeightedValue<string>("0## #### ####", 1),
        new WeightedValue<string>("###########", 1),
        new WeightedValue<string>("1##########", 1)
    ];
}