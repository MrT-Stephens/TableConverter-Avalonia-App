using Dock.Model.Controls;

namespace TableConverter.Interfaces
{
    public interface IPaneTool<TViewModel> : IPane<TViewModel>, ITool
        where TViewModel : IWorkspace
    {
    }
}
