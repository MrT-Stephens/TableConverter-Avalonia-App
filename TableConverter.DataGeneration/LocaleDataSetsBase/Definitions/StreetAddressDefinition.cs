using System.Collections.Immutable;
using TableConverter.DataGeneration.Interfaces;

namespace TableConverter.DataGeneration.LocaleDataSetsBase.Definitions;

public record StreetAddressDefinition(
    ImmutableArray<IWeightedValue<ITemplatedValueBuilder<FakerBase, LocaleBase>>> Normal,
    ImmutableArray<IWeightedValue<ITemplatedValueBuilder<FakerBase, LocaleBase>>> Full);