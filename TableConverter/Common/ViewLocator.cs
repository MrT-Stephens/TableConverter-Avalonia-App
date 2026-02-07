using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using TableConverter.Interfaces;

namespace TableConverter.Common;

public class ViewLocator : IDataTemplate
{
    private readonly Dictionary<object, Control> _controlCache = [];
    private readonly IViewsCollection _views;
    private readonly ILogger<ViewLocator> _logger;

    public ViewLocator(IViewsCollection views, ILogger<ViewLocator> logger)
    {
        _views = views;
        _logger = logger;
    }

    public Control Build(object? param)
    {
        if (param is null)
        {
            return CreateText("Data is null.");
        }

        if (_controlCache.TryGetValue(param, out var control))
        {
            _logger.LogDebug("Returning cached control for {0}", param.GetType().Name);
            return control;
        }

        if (!_views.TryCreateView(param, out var view))
        {
            _logger.LogError("No view found for {0}", param.GetType().Name);
            return CreateText($"No View For {param.GetType().Name}.");
        }
        
        _logger.LogDebug("Created view for {0}", param.GetType().Name);
        _controlCache.Add(param, view);

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