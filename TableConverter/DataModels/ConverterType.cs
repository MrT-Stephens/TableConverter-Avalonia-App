using System.Collections.ObjectModel;
using TableConverter.Interfaces;

namespace TableConverter.DataModels
{
    public record ConverterType(
        string name,
        Collection<string> extensions,
        Collection<string> mime_types,
        string description,
        bool convert_from,
        bool convert_to,
        IConverterHanderInput? input_converter,
        IConverterHandlerOutput? output_converter
    );
}