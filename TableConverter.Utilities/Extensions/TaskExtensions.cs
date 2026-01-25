namespace TableConverter.Utilities.Extensions;

public static class TaskExtensions
{
    public static void FireAndForget(this Task task)
    {
        var currentContext = TaskScheduler.FromCurrentSynchronizationContext();
        
        _ = task.ContinueWith(t =>
        {
            t.Exception?.CaptureAndThrow();
        }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, currentContext);
    }
}