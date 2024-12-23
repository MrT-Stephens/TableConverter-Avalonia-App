using System.Collections.Immutable;

namespace TableConverter.DataGeneration.LocaleDataSetsBase.Definitions;

public record HttpStatusCodeDefinition(
    ImmutableArray<string> Informational,
    ImmutableArray<string> Success,
    ImmutableArray<string> Redirection,
    ImmutableArray<string> ClientError,
    ImmutableArray<string> ServerError);