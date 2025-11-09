namespace TableConverter.Interfaces;

public interface IWorkspace
{
    public string Title { get; set; }
    
    public object Icon { get; set; }
    
    public int Index { get; set; }
    
    public bool IsBusy { get; set; }
        
    public string BusyText { get; set; }
        
    public void SetBusy(bool isBusy, string busyText = "Loading...");
        
    public void ClearBusy();
}
