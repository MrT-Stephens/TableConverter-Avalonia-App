using TableConverter.Utilities.Interfaces;

namespace TableConverter.Utilities;

public class TableDataColumn(string name, Type? dataType = null, object? defaultValue = null) : ITableDataColumn
{
    public virtual string Name { get; set; } = name;
    
    public virtual object DefaultValue { get; set; } = defaultValue ?? GetDefaultValue(dataType);
    
    public virtual Type DataType { get; set; } = dataType ?? typeof(string);
    
    public static object GetDefaultValue(Type? type)
    {
        type ??= typeof(string);
        
        type = Nullable.GetUnderlyingType(type) ?? type;
        
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type) ?? string.Empty;
        }
        
        return string.Empty;
    }
}