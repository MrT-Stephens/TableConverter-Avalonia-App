using System.Collections.Immutable;

namespace TableConverter.DataGeneration.LocaleDataSetsBase.Definitions;

public record SexDefinition(ImmutableArray<string> Generic, ImmutableArray<string> Male, ImmutableArray<string> Female);