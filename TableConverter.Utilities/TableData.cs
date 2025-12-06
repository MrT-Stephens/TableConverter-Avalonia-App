using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities;

public class TableData : ITableData
{
    #region Properties
    
    public virtual IList<ITableDataColumn> Columns { get; }
    
    public virtual IList<ITableDataRow> Rows { get; }

    public virtual bool IsEmpty => Rows.Count == 0;
    
    public virtual int RowCount => Rows.Count;
    
    public virtual int ColumnCount => Columns.Count;
    
    #endregion

    #region Constructors
    
    public TableData()
    {
        Columns = new List<ITableDataColumn>();
        Rows    = new List<ITableDataRow>();
    }

    public TableData(int initialColumnCapacity, int initialRowCapacity)
    {
        Columns = new List<ITableDataColumn>(initialColumnCapacity);
        Rows    = new List<ITableDataRow>(initialRowCapacity);
    }
    
    public TableData(IEnumerable<ITableDataColumn> columns, IEnumerable<ITableDataRow> rows)
    {
        ArgumentNullException.ThrowIfNull(columns, nameof(columns));
        ArgumentNullException.ThrowIfNull(rows, nameof(rows));
        
        Columns = new List<ITableDataColumn>(columns);
        Rows    = new List<ITableDataRow>(rows);
    }

    public TableData(IEnumerable<string> columnNames, IEnumerable<IEnumerable<object>> rows)
    {
        ArgumentNullException.ThrowIfNull(columnNames, nameof(columnNames));
        ArgumentNullException.ThrowIfNull(rows, nameof(rows));
        
        Columns = new List<ITableDataColumn>(columnNames.Select(c => new TableDataColumn(c)));
        Rows    = new List<ITableDataRow>(rows.Select(r => new TableDataRow(r)));
    }
    
    #endregion
    
    #region Public Methods
    
    public void AddColumn(string name, Type? type = null, object? defaultValue = null)
    {
        InsertColumn(Columns.Count, name, type, defaultValue);
    }
    
    public void AddColumn<T>(string name, T? defaultValue = default)
    {
        AddColumn(name, typeof(T), defaultValue);
    }

    public void InsertColumn(int index, string name, Type? type = null, object? defaultValue = null)
    {
        if (index < 0 || index > Columns.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        var column = CreateColumn(name, type, defaultValue);
        
        Columns.Insert(index, column);
        
        foreach (var row in Rows)
        {
            row.Insert(index, column.DefaultValue);
        }
    }

    public void InsertColumn<T>(int index, string name, T? defaultValue = default)
    {
        InsertColumn(index, name, typeof(T), defaultValue);
    }

    public bool RemoveColumn(string name)
    {
        var index = -1;

        for (var i = 0; i < Columns.Count; i++)
        {
            if (string.Equals(Columns[i].Name, name, StringComparison.OrdinalIgnoreCase))
            {
                index = i;
                break;
            }
        }

        if (index < 0)
        {
            return false;
        }

        RemoveColumnAt(index);
        return true;
    }

    public void RemoveColumnAt(int index)
    {
        if (index < 0 || index >= Columns.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        Columns.RemoveAt(index);
        
        foreach (var row in Rows)
        {
            row.RemoveAt(index);
        }
    }

    public void ReplaceColumn(int index, string name)
    {
        if (index < 0 || index >= Columns.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        
        Columns[index].Name = name;
    }

    public ITableDataRow AddRow()
    {
        var row = CreateRow();

        for (var i = 0; i < ColumnCount; i++)
        {
            row.Add(Columns[i].DefaultValue);
        }

        Rows.Add(row);
        return row;
    }

    public void AddRow(IEnumerable<object> values)
    {
        var counter = 0;
        var row = CreateRow();
        
        foreach (var value in values)
        {
            if (counter >= ColumnCount)
            {
                throw new InvalidOperationException(
                    $"Row column count exceeds table column count {ColumnCount}.");
            }

            row.Add(value);
            counter++;
        }

        Rows.Add(row);
    }

    public ITableDataRow InsertRow(int index)
    {
        if (index < 0 || index > Rows.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        
        var row = CreateRow();
        
        for (var i = 0; i < ColumnCount; i++)
        {
            row.Add(Columns[i].DefaultValue);
        }
        
        Rows.Insert(index, row);
        return row;
    }
    
    public void InsertRow(int index, IEnumerable<object> values)
    {
        if (index < 0 || index > Rows.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        var counter = 0;
        var row = CreateRow();
        
        foreach (var value in values)
        {
            if (counter >= ColumnCount)
            {
                throw new InvalidOperationException(
                    $"Row column count exceeds table column count {ColumnCount}.");
            }

            row.Add(value);
            counter++;
        }

        Rows.Insert(index, row);
    }

    public bool RemoveRow(ITableDataRow row)
    {
        return Rows.Remove(row);
    }

    public void RemoveRowAt(int index)
    {
        if (index < 0 || index >= Rows.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        Rows.RemoveAt(index);
    }

    public void ReplaceCell(int columnIndex, int rowIndex, object value)
    {
        if (rowIndex < 0 || rowIndex >= Rows.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(rowIndex));
        }

        if (columnIndex < 0 || columnIndex >= Columns.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(columnIndex));
        }
        
        Rows[rowIndex][columnIndex] = value;
    }

    public void Clear()
    {
        Columns.Clear();
        Rows.Clear();
    }

    public ITableData Clone()
    {
        var newTableData = CreateTableData();

        foreach (var column in Columns)
        {
            newTableData.AddColumn(column.Name, column.DataType, column.DefaultValue);
        }

        foreach (var row in Rows)
        {
            var newRow = newTableData.AddRow();
            
            for (var i = 0; i < ColumnCount; i++)
            {
                newRow[i] = row[i];
            }
        }

        return newTableData;
    }

    #endregion

    #region Miscellaneous

    protected virtual ITableDataColumn CreateColumn(string name, Type? type = null, object? defaultValue = null)
    {
        return new TableDataColumn(name, type, defaultValue);
    }

    protected virtual ITableDataRow CreateRow()
    {
        return new TableDataRow(ColumnCount);
    }

    protected virtual ITableData CreateTableData()
    {
        return new TableData();
    }

    #endregion
}