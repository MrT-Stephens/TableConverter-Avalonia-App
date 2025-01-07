using System.Collections.Immutable;
using TableConverter.DataGeneration.DataModels;
using TableConverter.DataGeneration.Interfaces;
using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.LocaleDataSets.zh_CN;

public class CompanyDataSet : en.CompanyDataSet
{
    public override ImmutableArray<string> Category { get; } =
    [
        "传媒",
        "保险",
        "印刷",
        "建设",
        "旅游发展",
        "林业",
        "水产",
        "燃气",
        "物流",
        "电力",
        "矿业",
        "网络科技",
        "运输",
        "食品"
    ];

    public override ImmutableArray<string> LegalEntitySuffix { get; } =
    [
        "无限公司",
        "无限责任公司",
        "有限公司",
        "有限责任公司",
        "股份有限公司",
        "集团有限公司",
        "（集团）有限公司"
    ];

    public override ImmutableArray<IWeightedValue<ITemplatedValueBuilder<FakerBase, LocaleBase>>> NamePattern { get; } =
    [
        new WeightedValue<ITemplatedValueBuilder<FakerBase, LocaleBase>>(
            new TemplatedValueBuilder<FakerBase, LocaleBase>()
                .SetTemplate("{City}{FirstName}{Category}{Suffix}")
                .AddPlaceholder("City", (faker, _, _) => faker.Location.City())
                .AddRandomPlaceholder("FirstName", dataset => dataset.Person.Value.FirstName.Generic)
                .AddRandomPlaceholder("Category", dataset => dataset.Company.Value.Category)
                .AddRandomPlaceholder("Suffix", dataset => dataset.Company.Value.LegalEntitySuffix),
            10
        ),
        new WeightedValue<ITemplatedValueBuilder<FakerBase, LocaleBase>>(
            new TemplatedValueBuilder<FakerBase, LocaleBase>()
                .SetTemplate("{State}{FirstName}{Category}{Suffix}")
                .AddPlaceholder("State", (faker, _, _) => faker.Location.State())
                .AddRandomPlaceholder("FirstName", dataset => dataset.Person.Value.FirstName.Generic)
                .AddRandomPlaceholder("Category", dataset => dataset.Company.Value.Category)
                .AddRandomPlaceholder("Suffix", dataset => dataset.Company.Value.LegalEntitySuffix),
            15
        )
    ];
}