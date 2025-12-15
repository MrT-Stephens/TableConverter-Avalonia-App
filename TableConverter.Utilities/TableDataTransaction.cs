using System.Collections.Specialized;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities;

public class TableDataTransaction : ITableDataTransaction
{
    #region Fields
    
    private readonly IList<string> _headers;
    private readonly IList<string[]> _rows;

    private readonly Dictionary<int, string> _headerChanges = new();
    private readonly Dictionary<int, string[]> _rowChanges = new();

    private bool _Committed;
    
    #endregion

    #region Constructors

    public TableDataTransaction(TableData tableData)
    {
        _headers = tableData.Headers;
        _rows = tableData.Rows;
    }
    
    public TableDataTransaction(IList<string> headers, IList<string[]> rows)
    {
        _headers = headers;
        _rows = rows;
    }

    #endregion

    #region Methods
    
    public string GetHeader(int index) => _headerChanges.ContainsKey(index) 
        ? _headerChanges[index] 
        : _headers[index];

    public void SetHeader(int index, string value) => _headerChanges[index] = value;

    public string GetCell(int row, int column)
    {
        if (_rowChanges.TryGetValue(row, out var copy))
        {
            return copy[column];
        }

        return _rows[row][column];
    }

    public void SetCell(int row, int column, string value)
    {
        if (!_rowChanges.TryGetValue(row, out var copy))
        {
            copy = (string[])_rows[row].Clone();
            _rowChanges[row] = copy;
        }

        copy[column] = value;
    }

    public void Commit()
    {
        // Apply header changes
        foreach (var kv in _headerChanges)
            _headers[kv.Key] = kv.Value;

        // Apply row changes
        foreach (var (key, changed) in _rowChanges)
        {
            _rows[key] = changed;
        }

        _Committed = true;
    }

    public void Dispose()
    {
        if (_Committed) return;
        
        _headerChanges.Clear();
        _rowChanges.Clear();
    }
    
    #endregion
}