using System.Collections.Generic;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.DataModels
{
    public record ConverterType(
        string Name,
        IReadOnlyList<string> Extensions,
        IReadOnlyList<string> MimeTypes,
        IReadOnlyList<string> AppleUTIs,
        string Description,
        IConverterHandlerInput? InputConverterHandler,
        IConverterHandlerOutput? OutputConverterHandler
    );
}