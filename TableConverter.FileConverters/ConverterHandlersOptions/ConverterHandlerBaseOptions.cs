using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace TableConverter.FileConverters.ConverterHandlersOptions;

public class ConverterHandlerBaseOptions : INotifyPropertyChanged
{
    public override string ToString()
    {
        var properties = GetType()
            .GetProperties()
            .Where(p => p is { CanRead: true, CanWrite: true })
            .ToList();

        if (properties.Count == 0) return $"No properties found in {GetType().Name}";

        var sb = new StringBuilder();

        foreach (var property in properties)
            sb.Append($"{property.Name}: {(property.GetValue(this) is { } value ? value.ToString() : "null")}\n");

        return sb.ToString();
    }

    #region INotifyPropertyChanged Members
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) 
            return false;
        
        field = value;
        OnPropertyChanged(propertyName);
        
        return true;
    }
    
    #endregion
}