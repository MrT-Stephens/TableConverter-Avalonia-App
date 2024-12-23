using System;
using System.Collections.Generic;

namespace TableConverter.DataModels;

public record DataGenerationMethod(
    string Name,
    string Description,
    IReadOnlyList<DataGenerationMethodParameter> Parameters,
    Action<object[]> ExecuteMethod);

public record DataGenerationMethodParameter(
    string Name,
    Type Type,
    object? DefaultValue);

public record DataGenerationType(
    string Name,
    string Description,
    IReadOnlyList<DataGenerationMethod> Methods);