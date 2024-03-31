using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TableConverter.Interfaces
{
    public interface IConverterHanderInput
    {
        public List<Avalonia.Controls.Control>? Controls { get; init; }

        public void InitializeControls();

        public Task<string> ReadExampleAsync();

        public Task<string> ReadFileAsync(IStorageFile storage_file);

        public Task<(List<string>, List<string[]>)> ReadTextAsync(string text);
    }
}
