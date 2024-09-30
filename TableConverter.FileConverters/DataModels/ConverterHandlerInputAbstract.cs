using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.DataModels
{
    public abstract class ConverterHandlerInputAbstract<T> : IConverterHanderInput where T : ConverterHandlerBaseOptions, new()
    {
        dynamic? IConverterHanderInput.Options
        {
            get => Options;
            set => Options = value;
        }

        public T? Options { get; set; }

        public ConverterHandlerInputAbstract()
        {
            Options = (typeof(T) == typeof(ConverterHandlerBaseOptions)) ? null : new T();
        }

        public virtual string ReadFile(Stream? stream)
        {
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));

            const int maxRetries = 3;
            int attempts = 0;

            using (var reader = new StreamReader(stream))
            {
                while (attempts < maxRetries)
                {
                    try
                    {
                        return reader.ReadToEnd();
                    }
                    catch (IOException ex) when (ex.Message.Contains("timed out"))
                    {
                        attempts++;
                        Thread.Sleep(2000); // Wait before retrying
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }

            return string.Empty;
        }

        public async Task<string> ReadFileAsync(Stream? stream) => await Task.Run(() => ReadFile(stream));

        public abstract TableData ReadText(string text);

        public async Task<TableData> ReadTextAsync(string text) => await Task.Run(() => ReadText(text));
    }
}
