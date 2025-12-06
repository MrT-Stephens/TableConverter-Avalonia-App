using System.Diagnostics.CodeAnalysis;

namespace TableConverter.Utilities.Interfaces;

public interface ITableDataRow : IEnumerable<object?>
{
    public IList<object?> Items { get; }
    
    public int Count { get; }
    
    public bool IsEmpty { get; }
    
    public object? this[int index] { get; set; }
    
    public void Add<T>(T? value);
    
    public void Insert<T>(int index, T? value);
    
    public void Remove<T>(T? value);
    
    public void RemoveAt(int index);
    
    public T? Get<T>(int index);
    
    public bool TryGet<T>(int index, [NotNullWhen(true)] out T? value);
    
    public void Clear();
}