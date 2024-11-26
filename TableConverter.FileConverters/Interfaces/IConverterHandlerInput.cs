using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.Interfaces
{
    public interface IConverterHandlerInput
    {
        public dynamic? Options { get; set; }

        public Result<string> ReadFile(Stream? stream);

        public Task<Result<string>> ReadFileAsync(Stream? stream);

        public Result<TableData> ReadText(string text);

        public Task<Result<TableData>> ReadTextAsync(string text);
    }
}
