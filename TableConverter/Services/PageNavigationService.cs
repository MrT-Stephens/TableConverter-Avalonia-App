using System;
using TableConverter.ViewModels;

namespace TableConverter.Services;

public class PageNavigationService
{
    public Action<Type, Action<BasePageViewModel>?>? NavigationRequested { get; set; }

    public void RequestNavigation<T>() where T : BasePageViewModel
    {
        NavigationRequested?.Invoke(typeof(T), null);
    }

    public void RequestNavigation<T>(Action<BasePageViewModel> setupAction) where T : BasePageViewModel
    {
        NavigationRequested?.Invoke(typeof(T), setupAction);
    }
}