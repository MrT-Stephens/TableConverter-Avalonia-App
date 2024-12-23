using System.Collections.Immutable;
using TableConverter.DataGeneration.DataModels;
using TableConverter.DataGeneration.Interfaces;
using TableConverter.DataGeneration.LocaleDataSetsBase;

namespace TableConverter.DataGeneration.LocaleDataSets.en_GB;

public class PersonDataSet : en.PersonDataSet
{
    public override ImmutableArray<IWeightedValue<ITemplatedValueBuilder<FakerBase, LocaleBase>>> NamePattern { get; } =
    [
        new WeightedValue<ITemplatedValueBuilder<FakerBase, LocaleBase>>(
            new TemplatedValueBuilder<FakerBase, LocaleBase>()
                .SetTemplate("{FirstName} {LastName}")
                .AddRandomPlaceholder("FirstName", dataset => dataset.Person.Value.FirstName.Generic)
                .AddRandomPlaceholder("LastName", dataset => dataset.Person.Value.LastName.Generic),
            60
        ),
        new WeightedValue<ITemplatedValueBuilder<FakerBase, LocaleBase>>(
            new TemplatedValueBuilder<FakerBase, LocaleBase>()
                .SetTemplate("{Title} {FirstName} {LastName}")
                .AddRandomPlaceholder("Title", dataset => dataset.Person.Value.Title.Generic)
                .AddRandomPlaceholder("FirstName", dataset => dataset.Person.Value.FirstName.Generic)
                .AddRandomPlaceholder("LastName", dataset => dataset.Person.Value.LastName.Generic),
            40
        )
    ];
}