using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.Interfaces;

namespace TableConverter.DataGeneration.DataModels
{
    public record DataGenerationField(
        string Name,
        string Type,
        ushort BlankPercentage,
        DataGenerationBaseOptions Options,
        IDataGenerationTypeHandler TypeHandler
    );
}
