using System.Collections.Immutable;

namespace TableConverter.DataGeneration.LocaleDataSetsBase.Definitions;

public record EmojiDefinition(
    ImmutableArray<string> Smiley,
    ImmutableArray<string> Body,
    ImmutableArray<string> Person,
    ImmutableArray<string> Nature,
    ImmutableArray<string> Food,
    ImmutableArray<string> Travel,
    ImmutableArray<string> Activity,
    ImmutableArray<string> Object,
    ImmutableArray<string> Symbol,
    ImmutableArray<string> Flag);