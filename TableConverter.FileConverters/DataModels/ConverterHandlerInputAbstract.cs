using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.DataModels;

public abstract class ConverterHandlerInputAbstract<T> : IConverterHandlerInput
    where T : ConverterHandlerBaseOptions, new()
{
    public T? Options { get; set; } = typeof(T) == typeof(ConverterHandlerBaseOptions) ? null : new T();

    dynamic? IConverterHandlerInput.Options
    {
        get => Options;
        set => Options = value;
    }

    public virtual Result<string> ReadFile(Stream? stream)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        const int maxRetries = 3;
        var attempts = 0;
        using var reader = new StreamReader(stream);

        while (attempts < maxRetries)
            try
            {
                return Result<string>.Success(reader.ReadToEnd());
            }
            catch (IOException exception) when (exception.Message.Contains("timed out"))
            {
                attempts++;
                Thread.Sleep(2000); // Wait before retrying
            }
            catch (Exception exception)
            {
                return Result<string>.Failure(exception.Message);
            }

        return Result<string>.Failure("Failed to read file");
    }

    public async Task<Result<string>> ReadFileAsync(Stream? stream)
    {
        return await Task.Run(() => ReadFile(stream));
    }

    public abstract Result<TableData> ReadText(string text);

    public async Task<Result<TableData>> ReadTextAsync(string text)
    {
        return await Task.Run(() => ReadText(text));
    }
}