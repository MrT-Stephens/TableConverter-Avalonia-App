namespace TableConverter.DataGeneration.DataModels
{
    public record DataGenerationType(
        string Name,
        string Category,
        string Description,
        Type GeneratorType
    );
}
