namespace TableConverter.DataModels
{
    public record ConverterType(
        string name,
        string extension,
        string description,
        bool convert_from,
        bool convert_to
    );
}