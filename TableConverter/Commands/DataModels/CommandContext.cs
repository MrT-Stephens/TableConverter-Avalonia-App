using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TableConverter.Commands.Interfaces;
using TableConverter.Common;
using TableConverter.Interfaces;
using TableConverter.Utilities;
using TableConverter.Utilities.Extensions;

namespace TableConverter.Commands.DataModels;

public class CommandContext : ICommandContext
{
    #region Properties
    
    /// <inheritdoc />
    public string Name { get; }
    
    /// <inheritdoc />
    public object? Parameter { get; set; }
    
    /// <inheritdoc />
    public object? Parent { get; set; }

    /// <inheritdoc />
    public Result<object>? Result { get; set; }
    
    /// <inheritdoc />
    public bool Cancelled { get; set; }
    
    /// <inheritdoc />
    public string CancelReason { get; set; }
    
    public SelectedItemsCollection SelectedItems { get; set; }
    
    #endregion

    #region Constructors

    public CommandContext(string commandName)
    {
        Name = commandName;
        Parameter = null;
        SelectedItems = [];
        Cancelled = false;
        CancelReason = string.Empty;
    }

    public CommandContext(string commandName, SelectedItemsCollection selectedItems)
    {
        Name = commandName;
        Parameter = null;
        SelectedItems = selectedItems;
        Cancelled = false;
        CancelReason = string.Empty;
    }
    
    #endregion

    #region IHasSelectedItems Implementation

    public bool TryGetSelectedItem<T>([NotNullWhen(true)] out T? item)
    {
        item = SelectedItems.GetSingle<T>();
        return item is not null;
    }
    
    public bool TryGetSelectedItems<T>(out IReadOnlyCollection<T> items)
    {
        items = SelectedItems.Get<T>();
        return items.Count > 0;
    }
    
    public void UpdateSelectedItemWith<T>(T item)
    {
        SelectedItems.RemoveAll<T>();
        SelectedItems.Add(item);
    }
    
    public void UpdateSelectedItemsWith<T>(IEnumerable<T> items)
    {
        SelectedItems.RemoveAll<T>();
        items.ForEach(x => SelectedItems.Add(x));
    }

    #endregion

    #region Methods

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

    /// <inheritdoc />
    public void Cancel(string reason = "")
    {
        Cancelled = true;
        CancelReason = reason;
    }

    #endregion
}