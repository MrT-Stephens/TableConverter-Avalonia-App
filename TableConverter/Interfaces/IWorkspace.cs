using TableConverter.Commands.DataModels;

namespace TableConverter.Interfaces;

public interface IWorkspace
{
    public string Title { get; set; }
    
    public object Icon { get; set; }
    
    public int Index { get; set; }
    
    public bool IsBusy { get; set; }
        
    public string BusyText { get; set; }
    
    /// <summary>
    /// The commands this workspace offers in the main menu, grouped and ordered as they are shown.
    /// </summary>
    public CommandMenu MainMenu { get; set; }
        
    public void SetBusy(bool isBusy, string busyText = "Loading...");
        
    public void ClearBusy();
}
