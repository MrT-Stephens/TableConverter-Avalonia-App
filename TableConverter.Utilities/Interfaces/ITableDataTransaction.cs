namespace TableConverter.Utilities.Interfaces;

public interface ITableDataTransaction : ITransaction
{
    public string GetHeader(int index);
    
    public void SetHeader(int index, string value);

    public string GetCell(int row, int column);
    
    public void SetCell(int row, int column, string value);
}