using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace TableConverter.Interfaces
{
    public abstract class ConverterHandlerOutputAbstract : IConverterHandlerOutput
    {
        public Collection<Control>? Controls { get; init; }

        protected ConverterHandlerOutputAbstract()
        {
            Controls = new Collection<Control>();

            InitializeControls();
        }

        public abstract void InitializeControls();

        public abstract Task<string> ConvertAsync(string[] headers, string[][] rows, object? progress_bar);

        public abstract Task SaveFileAsync(IStorageFile output, string data);
    }
}
