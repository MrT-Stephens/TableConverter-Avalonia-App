using System;
using TableConverter.Interfaces;
using TableConverter.ViewModels;

namespace TableConverter.Services;

public class PageNavigation : IPageNavigation
{
    public NavigationRequestedEventHandler? NavigationRequested { get; set; }

    public void RequestNavigation<T>() where T : BaseViewModel
    {
        NavigationRequested?.Invoke(typeof(T), null);
    }

    public void RequestNavigation<T>(Action<BaseViewModel> setupAction) where T : BaseViewModel
    {
        NavigationRequested?.Invoke(typeof(T), setupAction);
    }
}