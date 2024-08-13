using TableConverter.DataGeneration.DataGenerationOptions;

namespace TableConverter.DataGeneration.Interfaces
{
    public interface IDataGenerationTypeHandler<T> where T : class
    {
        public T? Options { get; init; }

        public string[] GenerateData(int rows, ushort blanks_percentage);

        public Task<string[]> GenerateDataAsync(int rows, ushort blanks_percentage);
    }
}
