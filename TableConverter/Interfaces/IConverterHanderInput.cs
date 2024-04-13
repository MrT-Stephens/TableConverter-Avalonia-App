using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace TableConverter.Interfaces
{
    internal interface IConverterHanderInput
    {
        public Collection<Control>? Controls { get; init; }

        public void InitializeControls();

        public Task<string> ReadFileAsync(IStorageFile storage_file);

        public Task<(List<string>, List<string[]>)> ReadTextAsync(string text);
    }
}
