namespace TableConverter.Utilities.Interfaces;

public interface ITableDataColumn
{
    public string Name { get; set; }
        
    public object DefaultValue { get; set; }
        
    public Type DataType { get; set; }
}