using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using TableConverter.Utilities.Extensions;

namespace TableConverter.Extensions;

public static class TaskExtensions
{
    public static void FireAndForget(this Task task)
    {
        _ = task.ContinueWith(t =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                t.Exception?.CaptureAndThrow();
            });
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
}