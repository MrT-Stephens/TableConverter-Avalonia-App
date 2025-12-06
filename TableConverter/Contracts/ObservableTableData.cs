using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TableConverter.Utilities;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Contracts;

public sealed class ObservableTableData : TableData
{
    #region Properties
    
    public ObservableCollection<ITableDataColumn> ObservableColumns { get; }
    public ObservableCollection<ITableDataRow> ObservableRows { get; }

    public override IList<ITableDataColumn> Columns => ObservableColumns;
    public override IList<ITableDataRow> Rows => ObservableRows;
    
    #endregion
    
    #region Constructors
    
    public ObservableTableData()
    {
        ObservableColumns = [];
        ObservableRows    = [];
    }

    public ObservableTableData(IEnumerable<string> columnNames, IEnumerable<IEnumerable<object>> rows)
    {
        ObservableColumns = [];
        ObservableRows    = [];
        
        foreach (var columnName in columnNames)
        {
            ObservableColumns.Add(CreateColumn(columnName));
        }
        
        foreach (var rowValues in rows)
        {
            var newRow = new TableDataRow(rowValues);
            ObservableRows.Add(newRow);
        }
    }
    
    public ObservableTableData(ITableData tableData)
        : this()
    {
        ArgumentNullException.ThrowIfNull(tableData, nameof(tableData));
        
        foreach (var column in tableData.Columns)
        {
            ObservableColumns.Add(CreateColumn(column.Name, column.DataType, column.DefaultValue));
        }
        
        foreach (var row in tableData.Rows)
        {
            var newRow = new TableDataRow(tableData.ColumnCount);
            
            for (var i = 0; i < ObservableColumns.Count; i++)
            {
                newRow[i] = row[i];
            }
            
            ObservableRows.Add(newRow);
        }
    }
    
    #endregion

    #region Overrides
    
    protected override ITableDataColumn CreateColumn(string name, Type? type = null, object? defaultValue = null)
    {
        return new ObservableTableDataColumn(name, type, defaultValue);
    }

    protected override ITableData CreateTableData()
    {
        return new ObservableTableData();
    }

    #endregion
}

public sealed class ObservableTableDataColumn(string name, Type? type = null, object? defaultValue = null) 
    : TableDataColumn(name, type, defaultValue), INotifyPropertyChanged
{
    #region Properties
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public override string Name
    {
        get => base.Name;
        set
        {
            if (base.Name != value)
            {
                base.Name = value;
                OnPropertyChanged();
            }
        }
    }
    
    public override object DefaultValue
    {
        get => base.DefaultValue;
        set
        {
            if (!Equals(base.DefaultValue, value))
            {
                base.DefaultValue = value;
                OnPropertyChanged();
            }
        }
    }
    
    public override Type DataType
    {
        get => base.DataType;
        set
        {
            if (base.DataType != value)
            {
                base.DataType = value;
                OnPropertyChanged();
            }
        }
    }
    
    #endregion
    
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
