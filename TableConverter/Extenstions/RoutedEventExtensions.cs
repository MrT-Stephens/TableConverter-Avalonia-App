using Avalonia.Interactivity;
using System;

namespace TableConverter.Extenstions;

public static class RoutedEventExtensions
{
    public static void AddHandler<TArgs, TControl>(
        this RoutedEvent<TArgs> routedEvent, 
        EventHandler<TArgs> handler, 
        params TControl?[] controls) 
        where TControl: Interactive
        where TArgs : RoutedEventArgs
    {
        foreach (var t in controls)
        {
            t?.AddHandler(routedEvent, handler);
        }
    }
    
    public static void RemoveHandler<TArgs, TControl>(
        this RoutedEvent<TArgs> routedEvent, 
        EventHandler<TArgs> handler, 
        params TControl?[] controls)
        where TArgs : RoutedEventArgs
        where TControl: Interactive
    {
        foreach (var t in controls)
        {
            t?.RemoveHandler(routedEvent, handler);
        }
    }
}