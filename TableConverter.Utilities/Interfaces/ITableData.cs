namespace TableConverter.Utilities.Interfaces
{
    public interface ITableData
    {
        public IEnumerable<string> GetHeaders();

        public IEnumerable<IEnumerable<object>> GetRows();
        
        public bool IsEmpty();
        
        public int GetRowCount();
        
        public int GetHeaderCount();
    }
}
