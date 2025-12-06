using System.Reflection;

namespace TableConverter.Utilities;

/// <summary>
/// Represents a weak reference to a delegate.
/// This allows the delegate to be garbage collected if there are no strong references to it.
/// </summary>
public class WeakDelegate
{
    private WeakReference? _Target;
    private readonly MethodInfo _methodInfo;
    private readonly Type _targetType;
    
    public WeakDelegate(Delegate method)
    {
        ArgumentNullException.ThrowIfNull(method, nameof(method));
        
        _Target = new WeakReference(method.Target);
        _methodInfo = method.Method;
        _targetType = method.GetType();
    }

    /// <summary>
    /// Gets the delegate if the target is still alive.
    /// </summary>
    public Delegate? Target => TryGetDelegate();

    /// <summary>
    /// Clears the weak reference to the target.
    /// </summary>
    public void Clear()
    {
        if (_Target is null) return;
        
        _Target.Target = null;
        _Target = null;
    }

    /// <summary>
    /// Tries to create the delegate if the target is still alive.
    /// </summary>
    /// <returns>
    /// The delegate if the target is still alive; otherwise, null.
    /// </returns>
    private Delegate? TryGetDelegate()
    {
        if (_methodInfo.IsStatic)
        {
            return Delegate.CreateDelegate(_targetType, null, _methodInfo);
        }

        if (_Target?.Target is { } target)
        {
            return Delegate.CreateDelegate(_targetType, target, _methodInfo);
        }

        return null;
    }
}