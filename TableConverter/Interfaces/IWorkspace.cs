using System.Collections.Generic;

namespace TableConverter.Interfaces;

public interface IWorkspace
{
    public string Header { get; set; }
    
    public object Icon { get; set; }
    
    public int Index { get; set; }
    
    public IEnumerable<IPane> Panes { get; }
}
