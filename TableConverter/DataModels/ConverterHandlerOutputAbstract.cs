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

        protected ConverterHandlerOutputAbstract()
        {
            Controls = new Collection<Control>();

            InitializeControls();
        }

        public abstract void InitializeControls();

        public abstract Task<string> ConvertAsync(string[] headers, string[][] rows, object? progress_bar);

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

        protected static void SetProgressBarValue(object? progress_bar, long value, long from_low, long from_high)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (progress_bar is ProgressBar bar)
                {
                    bar.Value = (Math.Max(from_low, Math.Min(from_high, value)) - from_low) / (from_high - from_low) * (bar.Maximum - bar.Minimum) + bar.Minimum;
                }
            }, DispatcherPriority.Render);
        }
    }
}
