namespace TableConverter.Utilities.Interfaces
{
    public interface ITableData
    {
        public IList<ITableDataColumn> Columns { get; }
        
        public int ColumnCount { get; }
        
        public void AddColumn(string name, Type? type = null, object? defaultValue = null);
        
        public void AddColumn<T>(string name, T? defaultValue = default);
        
        public void InsertColumn(int index, string name, Type? type = null, object? defaultValue = null);
        
        public void InsertColumn<T>(int index, string name, T? defaultValue = default);
        
        public bool RemoveColumn(string name);
        
        public void RemoveColumnAt(int index);

        public void ReplaceColumn(int index, string name);

        public int RowCount { get; }
        
        public IList<ITableDataRow> Rows { get; }
        
        public ITableDataRow AddRow();
        
        public void AddRow(IEnumerable<object> values);
        
        public ITableDataRow InsertRow(int index);
        
        public void InsertRow(int index, IEnumerable<object> values);
        
        public bool RemoveRow(ITableDataRow row);
        
        public void RemoveRowAt(int index);
        
        public void ReplaceCell(int columnIndex, int rowIndex, object value);
        
        public bool IsEmpty { get; }
        
        public void Clear();

        public ITableData Clone();
    }
}
