using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace TableConverter.Interfaces
{
    public interface IConverterHandlerOutput
    {
        public Collection<Control>? Controls { get; set; }

        public void InitializeControls();

        public Task<string> ConvertAsync(string[] headers, string[][] rows);

        public Task SaveFileAsync(IStorageFile output, ReadOnlyMemory<byte> buffer);
    }
}
