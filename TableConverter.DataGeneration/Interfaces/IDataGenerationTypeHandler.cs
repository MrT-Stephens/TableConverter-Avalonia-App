using TableConverter.DataGeneration.DataGenerationOptions;

namespace TableConverter.DataGeneration.Interfaces
{
    public interface IDataGenerationTypeHandler
    {
        public Type OptionsType { get; init; }

        public string[] GenerateData(int rows, DataGenerationBaseOptions? options, ushort blanks_percentage);

        public Task<string[]> GenerateDataAsync(int rows, DataGenerationBaseOptions? options, ushort blanks_percentage);
    }
}
