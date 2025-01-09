using TableConverter.FileConverters.DataModels;

namespace TableConverter.FileConverters.Interfaces;

public interface IConverterHandlerOutput
{
    public dynamic? Options { get; set; }

    public Result<string> Convert(string[] headers, string[][] rows);

    public Task<Result<string>> ConvertAsync(string[] headers, string[][] rows);

    public Result SaveFile(Stream? stream, ReadOnlyMemory<byte> buffer);

    public Task<Result> SaveFileAsync(Stream? stream, ReadOnlyMemory<byte> buffer);
}