using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using TableConverter.ViewModels.Forms;

namespace TableConverter.Interfaces
{
    public interface IWorkspaceEditor : IWorkspace
    {
        public ObservableCollection<IPaneDocument> Documents { get; set; }
        
        public ObservableCollection<IPaneTool> Tools { get; set; }
        
        public IPaneDocument? SelectedDocument { get; set; }
        
        public IPaneTool? SelectedTool { get; set; }
        
        public ToolsSettingsForm ToolsSettings { get; set; }

        public void Initialise();
        
        public void InitialiseTools();
        
        public void InitialiseDocuments();

        public void InitialiseEvents();

        public IPaneDocument CreateNewDocumentInstance();
    }
}
