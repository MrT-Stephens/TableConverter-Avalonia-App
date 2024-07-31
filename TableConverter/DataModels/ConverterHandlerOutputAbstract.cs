using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TableConverter.Interfaces;

namespace TableConverter.DataModels
{
    internal abstract class ConverterHandlerOutputAbstract : IConverterHandlerOutput
    {
        public Collection<Control>? Controls { get; set; }

        public abstract void InitializeControls();

        public abstract Task<string> ConvertAsync(string[] headers, string[][] rows);

        public virtual Task SaveFileAsync(IStorageFile output, ReadOnlyMemory<byte> buffer)
        {
            return Task.Run(async () =>
            {
                using (var writer = await output.OpenWriteAsync())
                {
                    writer.Write(buffer.Span);

                    writer.Close();
                }
            });
        }
    }
}
