using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;
using TableConverter.Utilities;
using TableConverter.Utilities.Extensions;

namespace TableConverter.Commands.DataModels;

public class CommandContext : ICommandContext
{
    /// <inheritdoc />
    public string Name { get; }
    
    /// <inheritdoc />
    public object? Parameter { get; set; }
    
    /// <inheritdoc />
    public object? ViewModel { get; set; }

    /// <inheritdoc />
    public Result<object>? Result { get; set; }

    /// <inheritdoc />
    public Dictionary<Type, object> SelectedItems { get; }

    public CommandContext(string commandName)
    {
        Name = commandName;
        Parameter = null;
        SelectedItems = new Dictionary<Type, object>();
    }

    public CommandContext(string commandName, Dictionary<Type, object> selectedItems)
    {
        Name = commandName;
        Parameter = null;
        SelectedItems = selectedItems;
    }

    /// <inheritdoc />
    public void UpdateSelectedItems<TObjectType>(TObjectType obj)
    {
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));

        if (SelectedItems.ContainsKey(typeof(TObjectType)))
        {
            SelectedItems[typeof(TObjectType)] = obj;
        }
        else
        {
            SelectedItems.Add(typeof(TObjectType), obj);
        }
    }

    /// <inheritdoc />
    public bool HasSelectedItem<TObjectType>()
    {
        return SelectedItems.ContainsKey(typeof(TObjectType));
    }

    /// <inheritdoc />
    public bool TryGetSelectedItem<TObjectType>([NotNullWhen(true)] out TObjectType? item)
    {
        if (SelectedItems.TryGetValue(typeof(TObjectType), out var value) && value is TObjectType typedItem)
        {
            item = typedItem;
            return true;
        }

        item = default;
        return false;
    }

    /// <inheritdoc />
    public void ClearSelectedItem<TObjectType>()
    {
        if (SelectedItems.ContainsKey(typeof(TObjectType)))
        {
            SelectedItems.Remove(typeof(TObjectType));
        }
    }

    /// <inheritdoc />
    public void ClearSelectedItems()
    {
        SelectedItems.Clear();
    }

    /// <inheritdoc />
    public void SetResult<TObjectType>(TObjectType obj)
    {
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));
        Result = Result<object>.Success(obj);
    }
    
    /// <inheritdoc />
    public void SetErrorResult(string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(errorMessage, nameof(errorMessage));
        Result = Result<object>.Failure(errorMessage);
    }

    /// <inheritdoc />
    public bool TryGetResult<TObjectType>([NotNullWhen(true)] out Result<TObjectType>? result)
    {
        if (Result is null)
        {
            result = Result<TObjectType>.Failure("No result is set");
            return false;
        }

        if (Result.Value is TObjectType objectType)
        {
            result = Result<TObjectType>.Success(objectType);
            return true;
        }
        
        result = Result<TObjectType>.Failure("Result is not of type {0}".Format(typeof(TObjectType).Name));
        return false;
    }
}