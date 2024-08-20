using System;
using System.Collections.ObjectModel;
using TableConverter.Interfaces;

namespace TableConverter.DataModels
{
    public record ConverterType(
        string Name,
        Collection<string> Extensions,
        Collection<string> MimeTypes,
        string Description,
        Type? InputConverterType,
        Type? OutputConverterType
    );
}