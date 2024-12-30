using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Interfaces;
using TableConverter.ViewModels;

namespace TableConverter.Common;

/// <summary>
///     Default implementation of the <see cref="IViewsCollection" /> interface for managing view-to-view model mappings.
/// </summary>
public class ViewsCollection : IViewsCollection
{
    private readonly Dictionary<Type, Type> _vmToViewMap = new();

    /// <inheritdoc />
    public IViewsCollection AddView<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        TView,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        TViewModel>(IServiceCollection services)
        where TView : ContentControl
        where TViewModel : ObservableObject
    {
        var viewType = typeof(TView);
        var viewModelType = typeof(TViewModel);

        _vmToViewMap.Add(viewModelType, viewType);

        if (viewModelType.IsAssignableTo(typeof(BasePageViewModel)))
            services.AddSingleton(typeof(BasePageViewModel), viewModelType);
        else
            services.AddSingleton(viewModelType);

        return this;
    }

    /// <inheritdoc />
    public bool TryCreateView(IServiceProvider provider, Type viewModelType, [NotNullWhen(true)] out Control? view)
    {
        view = null;

        var viewModel = provider.GetRequiredService(viewModelType);

        return TryCreateView(viewModel, out view);
    }

    /// <inheritdoc />
    public bool TryCreateView(object? viewModel, [NotNullWhen(true)] out Control? view)
    {
        view = null;

        if (viewModel == null) return false;

        var viewModelType = viewModel.GetType();

        if (_vmToViewMap.TryGetValue(viewModelType, out var viewType))
        {
            view = Activator.CreateInstance(viewType) as Control;

            if (view != null) view.DataContext = viewModel;
        }

        return view != null;
    }

    /// <inheritdoc />
    public Control CreateView<TViewModel>(IServiceProvider provider) where TViewModel : ObservableObject
    {
        var viewModelType = typeof(TViewModel);

        if (TryCreateView(provider, viewModelType, out var view)) return view;

        throw new InvalidOperationException($"Failed to create view for ViewModel type {viewModelType.FullName}.");
    }
}