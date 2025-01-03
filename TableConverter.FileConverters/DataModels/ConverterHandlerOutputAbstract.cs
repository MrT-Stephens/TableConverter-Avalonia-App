﻿using TableConverter.FileConverters.ConverterHandlersOptions;
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

        public T? Options { get; set; } = (typeof(T) == typeof(ConverterHandlerBaseOptions)) ? null : new T();

        public abstract Result<string> Convert(string[] headers, string[][] rows);

        public async Task<Result<string>> ConvertAsync(string[] headers, string[][] rows) => await Task.Run(() => Convert(headers, rows));

        public virtual Result SaveFile(Stream? stream, ReadOnlyMemory<byte> buffer)
        {
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));

            try
            {
                stream.Write(buffer.Span);

                stream.Close();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.Message);
            }
            
            return Result.Success();
        }

        public async Task<Result> SaveFileAsync(Stream? output, ReadOnlyMemory<byte> buffer) => await Task.Run(() => SaveFile(output, buffer));
    }
}
