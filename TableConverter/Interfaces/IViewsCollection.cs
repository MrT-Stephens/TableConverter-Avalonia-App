using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace TableConverter.Interfaces;

/// <summary>
///     Interface for managing and resolving views associated with view models.
/// </summary>
public interface IViewsCollection
{
    /// <summary>
    ///     Adds a mapping between a view and its corresponding view model.
    /// </summary>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <param name="services">The service collection to register the view model.</param>
    /// <returns>The current instance of the views collection for chaining.</returns>
    IViewsCollection AddView<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        TView,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        TViewModel>(IServiceCollection services)
        where TView : ContentControl
        where TViewModel : ObservableObject;

    /// <summary>
    ///     Attempts to create a view for the specified view model type.
    /// </summary>
    /// <param name="provider">The service provider for resolving dependencies.</param>
    /// <param name="viewModelType">The type of the view model.</param>
    /// <param name="view">The resulting view, if creation is successful.</param>
    /// <returns>True if the view was successfully created; otherwise, false.</returns>
    bool TryCreateView(IServiceProvider provider, Type viewModelType, [NotNullWhen(true)] out Control? view);

    /// <summary>
    ///     Attempts to create a view for the specified view model instance.
    /// </summary>
    /// <param name="viewModel">The view model instance.</param>
    /// <param name="view">The resulting view, if creation is successful.</param>
    /// <returns>True if the view was successfully created; otherwise, false.</returns>
    bool TryCreateView(object? viewModel, [NotNullWhen(true)] out Control? view);

    /// <summary>
    ///     Creates a view for the specified view model type using the service provider.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <param name="provider">The service provider for resolving dependencies.</param>
    /// <returns>The created view.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the view cannot be created.</exception>
    Control CreateView<TViewModel>(IServiceProvider provider) where TViewModel : ObservableObject;
}