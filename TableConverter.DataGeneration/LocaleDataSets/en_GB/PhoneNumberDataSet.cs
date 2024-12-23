using System.Collections.Immutable;
using TableConverter.DataGeneration.DataModels;
using TableConverter.DataGeneration.Interfaces;
using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.LocaleDataSets.en_GB;

public class PhoneNumberDataSet : PhoneNumberBase
{
    public override ImmutableArray<IWeightedValue<string>> HumanPattern { get; } =
    [
        new WeightedValue<string>("01#### #####", 1),
        new WeightedValue<string>("01### ######", 1),
        new WeightedValue<string>("01#1 ### ####", 1),
        new WeightedValue<string>("011# ### ####", 1),
        new WeightedValue<string>("02# #### ####", 1),
        new WeightedValue<string>("03## ### ####", 1),
        new WeightedValue<string>("055 #### ####", 1),
        new WeightedValue<string>("056 #### ####", 1),
        new WeightedValue<string>("0800 ### ####", 1),
        new WeightedValue<string>("08## ### ####", 1),
        new WeightedValue<string>("09## ### ####", 1),
        new WeightedValue<string>("016977 ####", 1),
        new WeightedValue<string>("01### #####", 1),
        new WeightedValue<string>("0500 ######", 1),
        new WeightedValue<string>("0800 ######", 1)
    ];

    public override ImmutableArray<IWeightedValue<string>> InternationalPattern { get; } =
    [
        new WeightedValue<string>("+441#########", 1),
        new WeightedValue<string>("+441#1#######", 1),
        new WeightedValue<string>("+4411########", 1),
        new WeightedValue<string>("+442#########", 1),
        new WeightedValue<string>("+443#########", 1),
        new WeightedValue<string>("+4455########", 1),
        new WeightedValue<string>("+4456########", 1),
        new WeightedValue<string>("+44800#######", 1),
        new WeightedValue<string>("+448#########", 1),
        new WeightedValue<string>("+449#########", 1),
        new WeightedValue<string>("+4416977####", 1),
        new WeightedValue<string>("+441########", 1),
        new WeightedValue<string>("+44500######", 1),
        new WeightedValue<string>("+44800######", 1)
    ];

    public override ImmutableArray<IWeightedValue<string>> NationalPattern { get; } =
    [
        new WeightedValue<string>("01### ######", 1),
        new WeightedValue<string>("01#1 ### ####", 1),
        new WeightedValue<string>("011# ### ####", 1),
        new WeightedValue<string>("02# #### ####", 1),
        new WeightedValue<string>("03## ### ####", 1),
        new WeightedValue<string>("055 #### ####", 1),
        new WeightedValue<string>("056 #### ####", 1),
        new WeightedValue<string>("0800 ### ####", 1),
        new WeightedValue<string>("08## ### ####", 1),
        new WeightedValue<string>("09## ### ####", 1),
        new WeightedValue<string>("016977 ####", 1),
        new WeightedValue<string>("01### #####", 1),
        new WeightedValue<string>("500######", 1),
        new WeightedValue<string>("0800 ######", 1)
    ];
}