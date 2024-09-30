using System.Collections.ObjectModel;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.DataModels
{
    public record ConverterType(
        string Name,
        Collection<string> Extensions,
        Collection<string> MimeTypes,
        Collection<string> AppleUTIs,
        string Description,
        IConverterHanderInput? InputConverterHandler,
        IConverterHandlerOutput? OutputConverterHandler
    );
}