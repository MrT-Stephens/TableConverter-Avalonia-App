using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TableConverter.Utilities.Database.Models.TableStore;

public sealed class SearchResult : EntityBase<int>
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
    
    private string _Value = null!;
    public string Value
    {
        get => _Value;
        set => SetField(ref _Value, value);
    }

    private string _FoundValue = null!;
    public string FoundValue
    {
        get => _FoundValue;
        set => SetField(ref _FoundValue, value);
    }
}