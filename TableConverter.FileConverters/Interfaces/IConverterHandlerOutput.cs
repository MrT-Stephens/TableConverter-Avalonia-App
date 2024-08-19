namespace TableConverter.FileConverters.Interfaces
{
    public interface IConverterHandlerOutput
    {
        public dynamic? Options { get; set; }

        public string Convert(string[] headers, string[][] rows);

        public Task<string> ConvertAsync(string[] headers, string[][] rows);

        public void SaveFile(Stream? stream, ReadOnlyMemory<byte> buffer);

        public Task SaveFileAsync(Stream? stream, ReadOnlyMemory<byte> buffer);
    }
}
