using System.Collections.Immutable;

namespace TableConverter.DataGeneration.LocaleDataSetsBase.Definitions;

public record DirectionDefinition(
    ImmutableArray<string> Cardinal,
    ImmutableArray<string> CardinalAbbr,
    ImmutableArray<string> Ordinal,
    ImmutableArray<string> OrdinalAbbr);