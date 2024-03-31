using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TableConverter.Interfaces
{
    public interface IConverterHandlerOutput
    {
        public List<Avalonia.Controls.Control>? Controls { get; init; }

        public void InitializeControls();

        public Task<string> ConvertAsync(string[] headers, string[][] rows, object? progress_bar);

        public Task SaveFileAsync(IStorageFile output, string data);
    }
}
