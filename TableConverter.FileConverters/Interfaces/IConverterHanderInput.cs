using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.Interfaces
{
    public interface IConverterHanderInput
    {
        public dynamic? Options { get; set; }

        public string ReadFile(Stream? stream);

        public Task<string> ReadFileAsync(Stream? stream);

        public TableData ReadText(string text);

        public Task<TableData> ReadTextAsync(string text);
    }
}
