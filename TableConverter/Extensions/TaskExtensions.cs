using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using TableConverter.Utilities.Extensions;

namespace TableConverter.Extensions;

public static class TaskExtensions
{
    /// <summary>
    ///     Observes a task that is intentionally not awaited, avoiding the <c>async void</c> anti-pattern.
    /// </summary>
    /// <remarks>
    ///     A faulted task is rethrown on the UI thread so the failure reaches the application's unhandled-exception
    ///     handling. Cancellation is treated as an expected, silent outcome.
    /// </remarks>
    /// <param name="task">The task to observe.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="task" /> is null.</exception>
    public static void FireAndForget(this Task task)
    {
        ArgumentNullException.ThrowIfNull(task);

        _ = ObserveAsync(task);
    }

    private static async Task ObserveAsync(Task task)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Cancellation is a normal outcome for work that is deliberately not awaited.
        }
        catch (Exception exception)
        {
            // Post rather than Invoke: Invoke marshals the rethrown exception back to this continuation, where no one
            // observes the resulting faulted task, so the error would be swallowed. Posting lets it surface on the
            // dispatcher so the application's unhandled-exception handling can see it.
            Dispatcher.UIThread.Post(() => exception.CaptureAndThrow());
        }
    }
}