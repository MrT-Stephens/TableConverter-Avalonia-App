using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace TableConverter.Interfaces
{
    public interface IPaneTool : IPane
    {
        public bool UseAutoScroll { get; set; }
    }
    
    public interface IScopedPaneTool<TWorkspace> : IPaneTool
    {
        public new TWorkspace Workspace { get; set; }
    }
}
