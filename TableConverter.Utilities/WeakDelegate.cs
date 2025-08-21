using System.Reflection;

namespace TableConverter.Utilities;

/// <summary>
/// Represents a weak reference to a delegate.
/// This allows the delegate to be garbage collected if there are no strong references to it.
/// </summary>
public class WeakDelegate
{
    private readonly WeakReference _Target;
    private readonly MethodInfo _MethodInfo;
    private readonly Type _TargetType;
    
    public WeakDelegate(Delegate method)
    {
        ArgumentNullException.ThrowIfNull(method, nameof(method));
        
        _Target = new WeakReference(method.Target);
        _MethodInfo = method.Method;
        _TargetType = method.Target!.GetType();
    }

    /// <summary>
    /// Gets the delegate if the target is still alive.
    /// </summary>
    public Delegate? Target => TryGetDelegate();

    private Delegate? TryGetDelegate()
    {
        if (_MethodInfo.IsStatic)
        {
            return Delegate.CreateDelegate(_TargetType, null, _MethodInfo);
        }

        if (_Target.Target is { } target)
        {
            return Delegate.CreateDelegate(_TargetType, target, _MethodInfo);
        }

        return null;
    }
}