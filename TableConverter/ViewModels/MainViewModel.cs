using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using TableConverter.Commands.Interfaces;
using TableConverter.Contracts;
using TableConverter.Interfaces;

namespace TableConverter.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    #region Properties

    [ObservableProperty] ObservableCollection<PagePane> _PagePanes;

    #endregion

    public MainViewModel(ICommandManager commandManager, IEventManager eventManager) 
        : base(commandManager, eventManager)
    {
        _commandManager.RegisterCommandInstance("AddFile", this);

        _PagePanes = new ObservableCollection<PagePane>
        {
            new PagePane("Converter", "Converter"),
            new PagePane("Data Generation", "Data Generation")
        };
    }
}
