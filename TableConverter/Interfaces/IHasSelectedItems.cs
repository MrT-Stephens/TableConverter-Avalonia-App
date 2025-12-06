using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TableConverter.Common;

namespace TableConverter.Interfaces;

public interface IHasSelectedItems
{
    public SelectedItemsCollection SelectedItems { get; set; }
    
    public bool TryGetSelectedItem<T>([NotNullWhen(true)] out T? item);
    
    public bool TryGetSelectedItems<T>(out IReadOnlyCollection<T> items);
    
    public void UpdateSelectedItemWith<T>(T item);
    
    public void UpdateSelectedItemsWith<T>(IEnumerable<T> items);
}