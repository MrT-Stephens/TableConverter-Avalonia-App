using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TableConverter.Utilities.Database.Models.TableStore;

public sealed class RowEntity : EntityBase<int>
{
    private ObservableCollection<CellEntity> _Cells = new();
    public ObservableCollection<CellEntity> Cells
    {
        get => _Cells;
        set => SetField(ref _Cells, value);
    }
}