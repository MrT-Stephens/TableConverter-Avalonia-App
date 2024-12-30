using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using TableConverter.Interfaces;

namespace TableConverter.Common;

public class ViewLocator : IDataTemplate
{
    private readonly IViewsCollection _views;
    private readonly Dictionary<object, Control> _controlCache = [];
    
    public ViewLocator(IViewsCollection views)
    {
        _views = views;
    }

    public Control Build(object? param)
    {
        if (param is null)
        {
            return CreateText("Data is null.");
        }

        if (_controlCache.TryGetValue(param, out var control))
        {
            return control;
        }

        if (_views.TryCreateView(param, out var view))
        {
            _controlCache.Add(param, view);

            return view;
        }

        return CreateText($"No View For {param.GetType().Name}.");
    }

    public bool Match(object? data) => data is ObservableObject;

    private static TextBlock CreateText(string text) => new TextBlock { Text = text };
}