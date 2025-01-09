using System.Collections.Immutable;

namespace TableConverter.DataGeneration.LocaleDataSetsBase.Definitions;

public record DateEntryDefinition(
    ImmutableArray<string> Wide,
    ImmutableArray<string> Abbreviated,
    ImmutableArray<string> WideContext,
    ImmutableArray<string> AbbreviatedContext);