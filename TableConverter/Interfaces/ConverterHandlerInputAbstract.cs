using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TableConverter.DataModels;

namespace TableConverter.Interfaces
{
    internal abstract class ConverterHandlerInputAbstract : IConverterHanderInput
    {
        public Collection<Control>? Controls { get; set; }

        protected ConverterHandlerInputAbstract()
        {
            Controls = new Collection<Control>();

            InitializeControls();
        }

        public virtual Task<string> ReadFileAsync(IStorageFile storage_file)
        {
            return Task.Run(async () =>
            {
                using (var reader = new System.IO.StreamReader(await storage_file.OpenReadAsync()))
                {
                    return await reader.ReadToEndAsync();
                }
            });
        }

        public abstract void InitializeControls();

        public abstract Task<TableData> ReadTextAsync(string text);
    }
}
