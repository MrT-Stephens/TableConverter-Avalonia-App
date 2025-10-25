using System.Collections.ObjectModel;
using TableConverter.Commands.Interfaces;
using TableConverter.ViewModels;

namespace TableConverter.Interfaces
{
    public interface IPane
    {
        public string Id { get; }

        public string Header { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsVisible { get; set; }
        
        public ObservableCollection<ICommandMetadata> ToolbarCommands { get; }
    }
}
