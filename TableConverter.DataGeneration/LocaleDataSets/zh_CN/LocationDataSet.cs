using System.Collections.Immutable;
using TableConverter.DataGeneration.DataModels;
using TableConverter.DataGeneration.Interfaces;
using TableConverter.DataGeneration.LocaleDataSetsBase;
using TableConverter.DataGeneration.LocaleDataSetsBase.Definitions;

namespace TableConverter.DataGeneration.LocaleDataSets.zh_CN;

public class LocationDataSet : en.LocationDataSet
{
    public override ImmutableArray<IWeightedValue<string>> BuildingNumberPattern { get; } =
    [
        new WeightedValue<string>("#", 50),
        new WeightedValue<string>("##", 40),
        new WeightedValue<string>("###", 30),
        new WeightedValue<string>("####", 20),
        new WeightedValue<string>("#####", 10)
    ];

    public override ImmutableArray<IWeightedValue<ITemplatedValueBuilder<FakerBase, LocaleBase>>> CityPattern { get; } =
    [
        new WeightedValue<ITemplatedValueBuilder<FakerBase, LocaleBase>>(
            new TemplatedValueBuilder<FakerBase, LocaleBase>()
                .SetTemplate("{{CityPrefix}}{{CitySuffix}}")
                .AddRandomPlaceholder("CityPrefix", dataset => dataset.Location.Value.CityPrefix)
                .AddRandomPlaceholder("CitySuffix", dataset => dataset.Location.Value.CitySuffix),
            1
        )
    ];

    public override ImmutableArray<string> CityPrefix { get; } =
    [
        "上",
        "包",
        "北",
        "南",
        "厦",
        "吉",
        "太",
        "宁",
        "安",
        "成",
        "武",
        "济",
        "海",
        "珠",
        "福",
        "衡",
        "西",
        "诸",
        "贵",
        "长"
    ];

    public override ImmutableArray<string> CitySuffix { get; } =
    [
        "乡县",
        "京市",
        "南市",
        "原市",
        "口市",
        "头市",
        "宁市",
        "安市",
        "州市",
        "徽市",
        "林市",
        "汉市",
        "沙市",
        "海市",
        "码市",
        "都市",
        "门市",
        "阳市"
    ];

    public override ImmutableArray<IWeightedValue<string>> PostCodePattern { get; } =
    [
        new WeightedValue<string>("######", 1)
    ];

    public override ImmutableArray<string> State { get; } =
    [
        "北京市",
        "上海市",
        "天津市",
        "重庆市",
        "黑龙江省",
        "吉林省",
        "辽宁省",
        "内蒙古自治区",
        "河北省",
        "新疆维吾尔自治区",
        "甘肃省",
        "青海省",
        "陕西省",
        "宁夏回族自治区",
        "河南省",
        "山东省",
        "山西省",
        "安徽省",
        "湖北省",
        "湖南省",
        "江苏省",
        "四川省",
        "贵州省",
        "云南省",
        "广西壮族自治区",
        "西藏自治区",
        "浙江省",
        "江西省",
        "广东省",
        "福建省",
        "海南省"
    ];

    public override ImmutableArray<string> StateAbbr { get; } =
    [
        "北京",
        "上海",
        "天津",
        "重庆",
        "黑龙江",
        "吉林",
        "辽阳",
        "内蒙古",
        "河北",
        "新疆",
        "甘肃",
        "青海",
        "陕西",
        "宁夏",
        "河南",
        "山东",
        "山西",
        "合肥",
        "湖北",
        "湖南",
        "苏州",
        "四川",
        "贵州",
        "云南",
        "广西",
        "西藏",
        "浙江",
        "江西",
        "广东",
        "福建",
        "海南"
    ];

    public override StreetAddressDefinition StreetAddress { get; } = new(
        [
            new WeightedValue<ITemplatedValueBuilder<FakerBase, LocaleBase>>(
                new TemplatedValueBuilder<FakerBase, LocaleBase>()
                    .SetTemplate("{Street}{BuildingNumber}号")
                    .AddPlaceholder("Street", (faker, _, _) => faker.Location.Street())
                    .AddPlaceholder("BuildingNumber", (faker, _, _) => faker.Location.BuildingNumber()),
                1
            )
        ],
        [
            new WeightedValue<ITemplatedValueBuilder<FakerBase, LocaleBase>>(
                new TemplatedValueBuilder<FakerBase, LocaleBase>()
                    .SetTemplate("{Street}{BuildingNumber}号 {SecondaryAddress}")
                    .AddPlaceholder("Street", (faker, _, _) => faker.Location.Street())
                    .AddPlaceholder("BuildingNumber", (faker, _, _) => faker.Location.BuildingNumber())
                    .AddPlaceholder("SecondaryAddress", (faker, _, _) => faker.Location.SecondaryAddress()),
                1
            )
        ]
    );

    public override ImmutableArray<IWeightedValue<ITemplatedValueBuilder<FakerBase, LocaleBase>>>
        StreetPattern { get; } =
    [
        new WeightedValue<ITemplatedValueBuilder<FakerBase, LocaleBase>>(
            new TemplatedValueBuilder<FakerBase, LocaleBase>()
                .SetTemplate("{LastName}{StreetSuffix}")
                .AddRandomPlaceholder("LastName", dataset => dataset.Person.Value.LastName.Generic)
                .AddRandomPlaceholder("StreetSuffix", dataset => dataset.Location.Value.StreetSuffix),
            1
        )
    ];

    public override ImmutableArray<string> StreetSuffix { get; } =
    [
        "巷", 
        "街", 
        "路", 
        "桥", 
        "侬", 
        "旁", 
        "中心", 
        "栋"
    ];
}