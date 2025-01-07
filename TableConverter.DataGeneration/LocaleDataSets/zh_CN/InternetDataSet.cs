using System.Collections.Immutable;
using TableConverter.DataGeneration.DataModels;
using TableConverter.DataGeneration.Interfaces;

namespace TableConverter.DataGeneration.LocaleDataSets.zh_CN;

public class InternetDataSet : en.InternetDataSet
{
    public override ImmutableArray<IWeightedValue<string>> FreeEmail { get; } =
    [
        new WeightedValue<string>("126.com", 1),
        new WeightedValue<string>("139.com", 1),
        new WeightedValue<string>("163.com", 1),
        new WeightedValue<string>("21cn.com", 1),
        new WeightedValue<string>("gmail.com", 1),
        new WeightedValue<string>("hotmail.com", 1),
        new WeightedValue<string>("qq.com", 1),
        new WeightedValue<string>("sina.com", 1),
        new WeightedValue<string>("sohu.com", 1),
        new WeightedValue<string>("tom.com", 1),
        new WeightedValue<string>("vip.qq.com", 1),
        new WeightedValue<string>("yahoo.cn", 1),
        new WeightedValue<string>("yahoo.com.cn", 1),
        new WeightedValue<string>("yeah.net", 1),
        new WeightedValue<string>("foxmail.com", 1),
        new WeightedValue<string>("outlook.com", 1)
    ];

    public override ImmutableArray<IWeightedValue<string>> DomainSuffix { get; } =
    [
        new WeightedValue<string>("cn", 30),
        new WeightedValue<string>("hk", 20),
        new WeightedValue<string>("mo", 20),
        new WeightedValue<string>("com", 10),
        new WeightedValue<string>("net", 10),
        new WeightedValue<string>("org", 10),
        new WeightedValue<string>("gov", 10),
        new WeightedValue<string>("edu", 10),
        new WeightedValue<string>("mil", 10),
        new WeightedValue<string>("info", 10),
        new WeightedValue<string>("biz", 10),
        new WeightedValue<string>("name", 10)
    ];
}