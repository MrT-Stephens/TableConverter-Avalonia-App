using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;

namespace TableConverter.ViewModels;

public class MainViewModel : BaseViewModel
{
    public MainViewModel(ICommandManager commandManager, IEventManager eventManager) 
        : base(commandManager, eventManager)
    {
        _commandManager.RegisterCommandInstance("AddFile", this);
    }
}
