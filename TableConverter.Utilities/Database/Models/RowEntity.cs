using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TableConverter.Utilities.Database.Models;

public sealed class RowEntity : INotifyPropertyChanged
{
    private long _RowId;
    public long RowId
    {
        get => _RowId; 
        set => SetField(ref _RowId, value);
    }

    private ObservableCollection<CellEntity> _Cells = [];
    public ObservableCollection<CellEntity> Cells
    {
        get => _Cells;
        set => SetField(ref _Cells, value);
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}