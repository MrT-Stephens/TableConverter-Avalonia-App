namespace TableConverter.Interfaces;

public interface IWorkspace
{
    public string Title { get; set; }
    
    public object Icon { get; set; }
    
    public int Index { get; set; }
}
