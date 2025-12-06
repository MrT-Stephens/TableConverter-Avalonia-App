using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TableConverter.Interfaces;
using TableConverter.Utilities.Extensions;

namespace TableConverter.Services;

public record PropertyChange(string PropertyName, object? OldValue, object? NewValue);

public class UndoRedo : IUndoRedo
{
    #region Properties
    
    private readonly int _maxHistoryPerObject;
    
    private readonly Dictionary<INotifyPropertyChanged, Stack<PropertyChange>> _undoStacks = new();
    private readonly Dictionary<INotifyPropertyChanged, Stack<PropertyChange>> _redoStacks = new();
    
    private readonly Dictionary<(INotifyPropertyChanged Target, string PropertyName), object?> _currentValues = new();
    
    private readonly Dictionary<(Type, string), PropertyInfo?> _propertyCache = new();
    
    #endregion

    #region Constructor

    public UndoRedo(int maxHistoryPerObject = 1000)
    {
        _maxHistoryPerObject = maxHistoryPerObject;
    }
    
    #endregion

    #region IUndoRedo Implementation

    public void Track(INotifyPropertyChanged target)
    {
        ArgumentNullException.ThrowIfNull(target, nameof(target));

        target.PropertyChanged -= OnTargetPropertyChanged;
        target.PropertyChanged += OnTargetPropertyChanged;

        if (!_undoStacks.ContainsKey(target))
            _undoStacks[target] = new Stack<PropertyChange>();
        if (!_redoStacks.ContainsKey(target))
            _redoStacks[target] = new Stack<PropertyChange>();

        foreach (var prop in target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead) continue;
            var value = prop.GetValue(target);
            _currentValues[(target, prop.Name)] = value;
        }
    }
    
    public void Untrack(INotifyPropertyChanged target)
    {
        ArgumentNullException.ThrowIfNull(target, nameof(target));
        
        target.PropertyChanged -= OnTargetPropertyChanged;
        
        _undoStacks.Remove(target);
        _redoStacks.Remove(target);
        
        var toRemove = _currentValues.Keys
            .Where(key => Equals(key.Target, target))
            .ToList();

        _currentValues.RemoveRange(toRemove);
    }

    private void OnTargetPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not INotifyPropertyChanged target || e.PropertyName == null) 
            return;
        
        var prop = GetProperty(target.GetType(), e.PropertyName);
            
        if (prop == null || !prop.CanRead) return;

        var newValue = prop.GetValue(target);
        var oldValue = _currentValues[(target, e.PropertyName)];

        if (!Equals(oldValue, newValue))
        {
            PushUndo(target, new PropertyChange(e.PropertyName, oldValue, newValue));
            _redoStacks[target].Clear();
            _currentValues[(target, e.PropertyName)] = newValue;
        }
    }

    private void PushUndo(INotifyPropertyChanged target, PropertyChange change)
    {
        var stack = _undoStacks[target];
        
        stack.Push(change);

        while (stack.Count > _maxHistoryPerObject)
        {
            var temp = new Stack<PropertyChange>();
            
            while (stack.Count > 1) temp.Push(stack.Pop());
            
            stack.Pop();
            
            while (temp.Count > 0) stack.Push(temp.Pop());
        }
    }

    public void Undo(INotifyPropertyChanged target)
    {
        if (!CanUndo(target)) return;

        var change = _undoStacks[target].Pop();
        
        SetProperty(target, change.PropertyName, change.OldValue);
        
        _redoStacks[target].Push(change);
    }

    public void Redo(INotifyPropertyChanged target)
    {
        if (!CanRedo(target)) return;

        var change = _redoStacks[target].Pop();
        
        SetProperty(target, change.PropertyName, change.NewValue);
        
        _undoStacks[target].Push(change);
    }

    public bool CanUndo(INotifyPropertyChanged target) =>
        _undoStacks.ContainsKey(target) && _undoStacks[target].Count > 0;

    public bool CanRedo(INotifyPropertyChanged target) =>
        _redoStacks.ContainsKey(target) && _redoStacks[target].Count > 0;
    
    #endregion

    #region Private Methods
    
    private void SetProperty(INotifyPropertyChanged target, string propertyName, object? value)
    {
        var prop = GetProperty(target.GetType(), propertyName);
        
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(target, value);
            _currentValues[(target, propertyName)] = value;
        }
    }

    private PropertyInfo? GetProperty(Type type, string propertyName)
    {
        var key = (type, propertyName);
        
        if (!_propertyCache.TryGetValue(key, out var prop))
        {
            prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            _propertyCache[key] = prop;
        }
        
        return prop;
    }
    
    #endregion
}