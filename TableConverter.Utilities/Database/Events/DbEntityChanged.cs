using TableConverter.Utilities.Models;

namespace TableConverter.Utilities.Database.Events;

[Flags]
public enum DbEntityChangeState
{
    None     = 0,
    Added    = 1 << 0,
    Modified = 1 << 1,
    Deleted  = 1 << 2
}

public sealed class DbEntityChangedEventArgs : EventArgs
{
    public Dictionary<Type, DbEntityChangeState> Changes { get; set; } = [];
}

public sealed class DbEntityChangedEvent : EventHandlerBase<DbEntityChangedEventArgs>
{
}