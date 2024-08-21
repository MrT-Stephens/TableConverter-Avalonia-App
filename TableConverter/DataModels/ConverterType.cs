using System.Collections.ObjectModel;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.DataModels
{
    public record ConverterType(
        string Name,
        Collection<string> Extensions,
        Collection<string> MimeTypes,
        string Description,
        IConverterHanderInput? InputConverterHandler,
        IConverterHandlerOutput? OutputConverterHandler
    );
}