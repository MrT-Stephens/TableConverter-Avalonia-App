using TableConverter.Interfaces;

namespace TableConverter.DataModels
{
    public record DataGenerationType(
        string Name,
        string Category,
        string Description
    );
}
