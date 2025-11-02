using Dock.Model.Controls;

namespace TableConverter.Interfaces
{
    public interface IPaneDocument<TViewModel> : IPane<TViewModel>, IDocument
        where TViewModel : IWorkspace
    {
        public bool IsDirty { get; set; }

        public bool IsReadOnly { get; set; }
    }
}
