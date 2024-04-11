using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Collections.ObjectModel;
using System.Text;
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

        public virtual async Task SaveFileAsync(IStorageFile output, ReadOnlyMemory<byte> buffer)
        {
            using (var stream = await output.OpenWriteAsync())
            {
                stream.Write(buffer.Span);
            }
        }

        protected static void SetProgressBarValue(object? progress_bar, double value, double from_low, double from_high)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                if (progress_bar is ProgressBar bar)
                {   
                    bar.Value = (Math.Max(from_low, Math.Min(from_high, value)) - from_low) / (from_high - from_low) * (bar.Maximum - bar.Minimum) + bar.Minimum;
                }
            }, DispatcherPriority.Render);
        }
    }
}
