using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TableConverter.Utilities.Database.Models.TableStore;

public sealed class ColumnEntity : INotifyPropertyChanged
{
    private int _ColumnId;
    public int ColumnId
    {
        get => _ColumnId;
        set => SetField(ref _ColumnId, value);
    }
    
    private string _Name = null!;
    public string Name
    {
        get => _Name;
        set => SetField(ref _Name, value);
    }

    private int _DataType;
    public int DataType
    {
        get => _DataType;
        set => SetField(ref _DataType, value);
    }

    private int _Ordinal;
    public int Ordinal
    {
        get => _Ordinal;
        set => SetField(ref _Ordinal, value);
    }

    private string? _DefaultValueForCell;
    public string? DefaultValueForCell
    {
        get => _DefaultValueForCell;
        set => SetField(ref _DefaultValueForCell, value);
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