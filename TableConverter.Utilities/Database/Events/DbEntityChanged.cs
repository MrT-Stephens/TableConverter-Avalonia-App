using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Utilities.Models;

namespace TableConverter.Utilities.Database.Events;

public enum DbEntityChangeState
{
    None     = 0,
    Added    = 1,
    Modified = 2,
    Deleted  = 3
}

public readonly struct DbEntityChange(object entity, DbEntityChangeState state)
{
    public object Entity { get; } = entity;
    
    public DbEntityChangeState State { get; } = state;
}

public sealed class DbEntityChangedEventArgs : EventArgs 
{
    public required Guid SourceId { get; init; }
    
    public required Type Type { get; init; }
    
    public required DbEntityChange[] Changes { get; init; }
}

public sealed class DbEntityChangedEvent : EventHandlerBase<DbEntityChangedEventArgs>
{
}