using System;
using CommunityToolkit.Mvvm.ComponentModel;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;

namespace TableConverter.ViewModels;

public partial class BasePageViewModel : BaseViewModel
{
    #region Constructors

    public BasePageViewModel(ICommandManager commandManager, IEventManager eventManager) 
        : base(commandManager, eventManager)
    {
    }

    #endregion
    
    #region Properties
    
    #endregion
}