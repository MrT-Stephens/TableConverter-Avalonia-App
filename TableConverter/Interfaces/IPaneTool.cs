using System.Diagnostics.CodeAnalysis;

namespace TableConverter.Interfaces
{
    public interface IPaneTool : IPane
    {
    }
    
    public interface IScopedPaneTool<TWorkspace> : IPaneTool
    {
        public new TWorkspace Workspace { get; set; }
    }
}
