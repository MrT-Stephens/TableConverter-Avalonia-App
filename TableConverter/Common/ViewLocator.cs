using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using TableConverter.Interfaces;

namespace TableConverter.Common;

public class ViewLocator : IDataTemplate
{
    private readonly Dictionary<object, Control> _ControlCache = [];
    private readonly IViewsCollection _Views;

    public ViewLocator(IViewsCollection views)
    {
        _Views = views;
    }

    public Control Build(object? param)
    {
        if (param is null)
        {
            return CreateText("Data is null.");
        }

        if (_ControlCache.TryGetValue(param, out var control))
        {
            return control;
        }

        if (!_Views.TryCreateView(param, out var view))
        {
            return CreateText($"No View For {param.GetType().Name}.");
        }

        _ControlCache.Add(param, view);

        return view;

    }

    public bool Match(object? data)
    {
        return data is ObservableObject;
    }

    private static TextBlock CreateText(string text)
    {
        return new TextBlock { Text = text };
    }
}