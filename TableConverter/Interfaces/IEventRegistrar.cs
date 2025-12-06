using System;

namespace TableConverter.Interfaces;

public interface IEventRegistrar : IDisposable
{
    public IEventRegistration Register(IEventHandle handle, object? owner = null);

    public void Clear(object owner);

    public void ClearAll();
}

public interface IEventRegistration : IDisposable
{
    public object? Owner { get; }
    public IEventHandle Handle { get; }
}

public interface IEventHandle
{
    public void Add();
    public void Remove();
}
