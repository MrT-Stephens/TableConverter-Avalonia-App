using System;
using System.Collections.Generic;

namespace TableConverter.DataModels;

public record DataGenerationMethod(
    string Key,
    string Name,
    string Description,
    IReadOnlyList<DataGenerationMethodParameter> Parameters);

public record DataGenerationMethodParameter(
    string Name,
    Type Type,
    object? DefaultValue);

public record DataGenerationType(
    string Name,
    string Description,
    object Icon,
    IReadOnlyList<DataGenerationMethod> Methods);