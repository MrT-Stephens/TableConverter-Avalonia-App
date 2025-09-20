using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;

namespace TableConverter.ViewModels;

public class MainViewModel : BaseViewModel
{
    public MainViewModel(ICommandManager commandManager, IEventManager eventManager) 
        : base(commandManager, eventManager)
    {
    }
}
