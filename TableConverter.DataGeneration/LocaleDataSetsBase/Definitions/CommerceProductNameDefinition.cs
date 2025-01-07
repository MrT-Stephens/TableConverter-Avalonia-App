using System.Collections.Immutable;

namespace TableConverter.DataGeneration.LocaleDataSetsBase.Definitions;

public record CommerceProductNameDefinition(
    ImmutableArray<string> Adjective,
    ImmutableArray<string> Material,
    ImmutableArray<string> Product
);