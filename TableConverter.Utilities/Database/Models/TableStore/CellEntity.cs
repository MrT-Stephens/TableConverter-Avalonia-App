using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TableConverter.Utilities.Database.Models.TableStore;

public sealed class CellEntity : EntityBase<int>
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
    
    private RowEntity? _Row;
    public RowEntity? Row
    {
        get => _Row;
        set => SetField(ref _Row, value);
    }
    
    private ColumnEntity? _Column;
    public ColumnEntity? Column
    {
        get => _Column;
        set => SetField(ref _Column, value);
    }
}