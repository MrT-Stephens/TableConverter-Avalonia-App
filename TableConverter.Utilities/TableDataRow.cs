using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities;

public class TableDataRow : ITableDataRow
{
    #region Properties

    public virtual IList<object?> Items { get; }
    
    public int Count => Items.Count;

    public bool IsEmpty => Items.Count == 0;

    #endregion

    #region Constructors
    
    public TableDataRow()
    {
        Items = new List<object?>();
    }
    
    public TableDataRow(int initialCapacity)
    {
        Items = new List<object?>(initialCapacity);
    }
    
    public TableDataRow(IEnumerable<object> items)
    {
        ArgumentNullException.ThrowIfNull(items, nameof(items));
        Items = new List<object?>(items);
    }

    #endregion

    #region Methods
    
    public object? this[int index]
    {
        get => Items[index];
        set => Items[index] = value;
    }
    
    public void Add<T>(T? value) => Items.Add(value);
    
    public void Insert<T>(int index, T? value) => Items.Insert(index, value);
    
    public void Remove<T>(T? value) => Items.Remove(value);
    
    public void RemoveAt(int index) => Items.RemoveAt(index);
    
    public T? Get<T>(int index) => (T?)Items[index];
    
    public bool TryGet<T>(int index, [NotNullWhen(true)] out T? value)
    {
        if (Items[index] is T castValue)
        {
            value = castValue;
            return true;
        }

        value = default;
        return false;
    }
    
    public void Clear() => Items.Clear();
    
    public IEnumerator<object?> GetEnumerator() => Items.GetEnumerator();
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}