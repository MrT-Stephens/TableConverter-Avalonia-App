using System.Collections.ObjectModel;
using TableConverter.Interfaces;

namespace TableConverter.DataModels
{
    public record ConverterType(
        string name,
        Collection<string> extensions,
        Collection<string> mimeTypes,
        string description,
        IConverterHanderInput? inputConverter,
        IConverterHandlerOutput? outputConverter
    );
}