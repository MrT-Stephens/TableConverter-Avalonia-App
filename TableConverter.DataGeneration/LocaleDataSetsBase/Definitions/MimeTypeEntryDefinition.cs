using System.Collections.Immutable;

namespace TableConverter.DataGeneration.LocaleDataSetsBase.Definitions;

public record MimeTypeEntryDefinition(string Mime, ImmutableArray<string> Extensions);