using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;

namespace TableConverter.Utilities.Extensions;

public static class ExceptionExtensions
{
    /// <summary>
    /// Rethrows the exception while preserving the original stack trace.
    /// </summary>
    /// <param name="exception">
    /// The exception to be rethrown.
    /// </param>
    [DoesNotReturn]
    public static void CaptureAndThrow(this Exception exception)
    {
        ExceptionDispatchInfo.Capture(exception).Throw();
    }
}