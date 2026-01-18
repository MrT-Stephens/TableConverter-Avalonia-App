using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TableConverter.Utilities.Database.Models.TableStore;

public sealed class ColumnEntity : EntityBaseWithAutoSynchronize<int> 
{
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
}