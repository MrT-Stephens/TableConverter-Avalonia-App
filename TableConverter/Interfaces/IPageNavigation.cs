using System;
using TableConverter.ViewModels;

namespace TableConverter.Interfaces;

public delegate void NavigationRequestedEventHandler(
    Type viewModelType, Action<BaseViewModel>? setupAction);

public interface IPageNavigation
{
    /// <summary>
    /// Requests navigation to a specific page view model type.
    /// </summary>
    public NavigationRequestedEventHandler? NavigationRequested { get; set; }

    /// <summary>
    /// Requests navigation to a specific page view model type.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the page view model to navigate to.
    /// </typeparam>
    public void RequestNavigation<T>() where T : BaseViewModel;
    
    /// <summary>
    /// Requests navigation to a specific page view model type with a setup action.
    /// </summary>
    /// <param name="setupAction">
    /// An action to set up the view model before navigation.
    /// </param>
    /// <typeparam name="T">
    /// The type of the page view model to navigate to.
    /// </typeparam>
    public void RequestNavigation<T>(Action<BaseViewModel> setupAction) where T : BaseViewModel;
}