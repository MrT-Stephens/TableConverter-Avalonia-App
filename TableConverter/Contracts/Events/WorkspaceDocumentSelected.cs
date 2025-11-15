using System;
using TableConverter.Interfaces;

namespace TableConverter.Contracts.Events;

public class WorkspaceDocumentSelectedEventArgs : EventArgs
{
    public required IWorkspaceEditor Workspace { get; set; }
    
    public required IPaneDocument? OldDocument { get; set; }
    
    public required IPaneDocument? NewDocument { get; set; }
}

public class WorkspaceDocumentSelectedEvent : EventHandlerBase<WorkspaceDocumentSelectedEventArgs>
{
}