using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TableConverter.Utilities.Database.Models.TableStore;

public sealed class CellEntity : INotifyPropertyChanged
{
    private int _RowId;
    public int RowId
    {
        get => _RowId;
        set => SetField(ref _RowId, value);
    }

    private int _ColumnId;
    public int ColumnId
    {
        get => _ColumnId;
        set => SetField(ref _ColumnId, value);
    }
    
    private string? _Value;
    public string? Value
    {
        get => _Value;
        set => SetField(ref _Value, value);
    }
    
    private RowEntity? _Row = null!;
    public RowEntity? Row
    {
        get => _Row;
        set => SetField(ref _Row, value);
    }
    
    private ColumnEntity? _Column = null!;
    public ColumnEntity? Column
    {
        get => _Column;
        set => SetField(ref _Column, value);
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