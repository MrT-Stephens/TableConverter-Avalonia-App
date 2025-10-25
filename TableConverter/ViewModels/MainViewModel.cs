using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;
using TableConverter.Utilities.Extensions;

namespace TableConverter.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    #region Properties

    [ObservableProperty] private ObservableCollection<IPane> _PagePanes;

    #endregion

    public MainViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager,
        IEnumerable<IPane> paneViewModels) 
        : base(commandManager, eventManager)
    {
        _PagePanes = paneViewModels.ToObservableCollection();
        
        _commandManager.RegisterCommandInstance("AddFile", this);
    }
}
