using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;

namespace TableConverter.Interfaces
{
    public interface IWorkspaceEditor : IWorkspace
    {
        public ObservableCollection<IPaneDocument> Documents { get; set; }
        
        public ObservableCollection<IPaneTool> Tools { get; set; }
        
        public IPaneDocument? SelectedDocument { get; set; }
        
        public IPaneTool? SelectedTool { get; set; }
        
        public Dock ToolsPosition { get; set; }

        public void AddNewDocument<TViewModel>(string title, Action<TViewModel>? initializeAction = null)
            where TViewModel : IPaneDocument;
        
        public void InitialiseTools();
    }
}
