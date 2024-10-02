using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TableConverter.Common;

public class ViewLocator : IDataTemplate
{
    private readonly Dictionary<object, Control> ControlCache = new();

    public Control Build(object? data)
    {
        var fullName = data?.GetType().FullName;

        if (fullName is null)
        {
            return new TextBlock()
            { 
                Text = "ViewLocator data is null or has no name."
            };
        }
        
        var name = fullName.Replace("ViewModel", "View");

        var type = Type.GetType(name);

        if (type is null)
        {
            return new TextBlock()
            { 
                Text = $"No View For {name}." 
            };
        }

        if (!ControlCache.TryGetValue(data!, out Control? view))
        {
            view ??= (Control)Activator.CreateInstance(type)!;

            ControlCache[data!] = view;
        }

        view.DataContext = data;

        return view;
    }

    public bool Match(object? data) => data is INotifyPropertyChanged;
}