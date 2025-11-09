using System.Diagnostics.CodeAnalysis;

namespace TableConverter.Interfaces
{
    public interface IPaneTool : IPane
    {
        public object Workspace { get; set; }
        
        public object? SelectedItem { get; set; }
        
        public bool TryGetSelectedItem<T>([NotNullWhen(true)] out T? item);
    }
    
    public interface IScopedPaneTool<TWorkspace> : IPaneTool
    {
    }
}
