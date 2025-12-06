using System.ComponentModel;
using System.Data;

namespace TableConverter.Interfaces;

public interface IUndoRedo
{
    public void Track(INotifyPropertyChanged target);
    
    public void Untrack(INotifyPropertyChanged target);
    
    public void Undo(INotifyPropertyChanged target);
    
    public void Redo(INotifyPropertyChanged target);
    
    public bool CanUndo(INotifyPropertyChanged target);
    
    public bool CanRedo(INotifyPropertyChanged target);
}
