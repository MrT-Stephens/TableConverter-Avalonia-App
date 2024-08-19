using TableConverter.FileConverters.ConverterHandlersOptions;
using TableConverter.FileConverters.Interfaces;

namespace TableConverter.FileConverters.DataModels
{
    public abstract class ConverterHandlerOutputAbstract<T> : IConverterHandlerOutput where T : ConverterHandlerBaseOptions, new()
    {
        dynamic? IConverterHandlerOutput.Options
        {
            get => Options;
            set => Options = value;
        }

        public T? Options { get; set; }

        public ConverterHandlerOutputAbstract()
        {
            Options = (typeof(T) == typeof(ConverterHandlerBaseOptions)) ? null : new T();
        }

        public abstract string Convert(string[] headers, string[][] rows);

        public async Task<string> ConvertAsync(string[] headers, string[][] rows) => await Task.Run(() => Convert(headers, rows));

        public virtual void SaveFile(Stream? stream, ReadOnlyMemory<byte> buffer)
        {
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));

            stream.Write(buffer.Span);

            stream.Close();
        }

        public async Task SaveFileAsync(Stream? output, ReadOnlyMemory<byte> buffer) => await Task.Run(() => SaveFile(output, buffer));
    }
}
