using System;
using TableConverter.Interfaces;
using TableConverter.ViewModels;

namespace TableConverter.Services;

public class PageNavigation : IPageNavigation
{
    public NavigationRequestedEventHandler? NavigationRequested { get; set; }

    public void RequestNavigation<T>() where T : BasePageViewModel
    {
        NavigationRequested?.Invoke(typeof(T), null);
    }

    public void RequestNavigation<T>(Action<BasePageViewModel> setupAction) where T : BasePageViewModel
    {
        NavigationRequested?.Invoke(typeof(T), setupAction);
    }
}