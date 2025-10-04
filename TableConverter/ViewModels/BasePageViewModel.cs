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
        Title = string.Empty;
        Id = Guid.NewGuid().ToString();
    }
    
    public BasePageViewModel(string title, string id, ICommandManager commandManager, IEventManager eventManager) 
        : this(commandManager, eventManager)
    {
        Title = title;
        Id = id;
    }

    #endregion
    
    #region Properties

    [ObservableProperty] private string _Title;
    [ObservableProperty] private string _Id;
    [ObservableProperty] private bool _IsBusy;
    
    #endregion
}