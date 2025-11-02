using System.Collections.ObjectModel;

namespace TableConverter.Interfaces
{
    public interface IWorkspaceEditor<TViewModel> : IWorkspace 
        where TViewModel : IWorkspace
    {
        public ObservableCollection<IPaneDocument<TViewModel>> Documents { get; set; }

        public ObservableCollection<IPaneTool<TViewModel>> Tools { get; set; }
    }
}
