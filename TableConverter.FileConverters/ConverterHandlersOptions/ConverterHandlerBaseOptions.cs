using System.Reflection;
using System.Text;

namespace TableConverter.FileConverters.ConverterHandlersOptions;

public class ConverterHandlerBaseOptions
{
    public override string ToString()
    {
        var properties = GetType()
            .GetProperties()
            .Where(p => p is { CanRead: true, CanWrite: true })
            .ToList();

        if (properties.Count == 0) return $"No properties found in {GetType().Name}";

        var sb = new StringBuilder();

        foreach (var property in properties) sb.Append($"{property.Name}: {(property.GetValue(this) is {} value ? value.ToString() : "null")}\n");

        return sb.ToString();
    }
}