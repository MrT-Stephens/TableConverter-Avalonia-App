using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities;

public class EventRegistrar : IEventRegistrar
{
    private readonly Dictionary<object, List<IEventRegistration>> _byOwner = [];
    private readonly List<IEventRegistration> _ownerless = [];

    public IEventRegistration Register(IEventHandle handle, object? owner = null)
    {
        var reg = new EventRegistration(handle, owner);

        if (owner is null)
        {
            _ownerless.Add(reg);
        }
        else
        {
            if (!_byOwner.TryGetValue(owner, out var list))
            {
                list = [];
                _byOwner[owner] = list;
            }
            
            list.Add(reg);
        }

        return reg;
    }

    public void Clear(object owner)
    {
        if (!_byOwner.TryGetValue(owner, out var list)) 
            return;

        foreach (var eventRegistration in list)
        {
            eventRegistration.Dispose();
        }

        _byOwner.Remove(owner);
    }

    public void ClearAll()
    {
        foreach (var eventRegistration in _byOwner.Values.SelectMany(list => list))
        {
            eventRegistration.Dispose();
        }

        foreach (var eventRegistration in _ownerless)
        {
            eventRegistration.Dispose();
        }

        _byOwner.Clear();
        _ownerless.Clear();
    }

    public void Dispose() => ClearAll();
}

public sealed class EventRegistration : IEventRegistration
{
    public object? Owner { get; }
    
    public IEventHandle Handle { get; }

    public EventRegistration(IEventHandle handle, object? owner = null)
    {
        Handle = handle;
        Owner = owner;
        handle.Add();
    }

    public void Dispose() => Handle.Remove();
}