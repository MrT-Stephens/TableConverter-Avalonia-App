using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using TableConverter.Interfaces;

namespace TableConverter.Common;

public class ViewLocator : IDataTemplate
{
    private readonly Dictionary<object, Control> _controlCache = [];
    private readonly IViewsCollection _views;

    public ViewLocator(IViewsCollection views)
    {
        _views = views;
    }

    public Control Build(object? param)
    {
        if (param is null) return CreateText("Data is null.");

        if (_controlCache.TryGetValue(param, out var control)) return control;

        if (_views.TryCreateView(param, out var view))
        {
            _controlCache.Add(param, view);

            return view;
        }

        return CreateText($"No View For {param.GetType().Name}.");
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