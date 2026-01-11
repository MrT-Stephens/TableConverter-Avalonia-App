using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using TableConverter.Commands.Interfaces;
using TableConverter.Common;
using TableConverter.Interfaces;
using TableConverter.Utilities;
using TableConverter.Utilities.Collections;
using TableConverter.Utilities.Extensions;

namespace TableConverter.Commands.DataModels;

public partial class CommandContext : ObservableObject, ICommandContext
{
    #region Properties
    
    [ObservableProperty] private string _Name;

    [ObservableProperty] private object? _Parameter;

    [ObservableProperty] private object? _Parent;

    [ObservableProperty] private Result<object>? _Result;

    [ObservableProperty] private bool _Cancelled;

    [ObservableProperty] private string _CancelReason;

    [ObservableProperty] private bool _IsLoading;
    
    [ObservableProperty] private bool _IsProcessing;

    [ObservableProperty] private SelectedItemsCollection _SelectedItems;
    
    #endregion

    #region Constructors

    public CommandContext(string commandName)
    {
        Name = commandName;
        Parameter = null;
        SelectedItems = [];
        Cancelled = false;
        IsLoading = false;
        CancelReason = string.Empty;
    }

    public CommandContext(string commandName, SelectedItemsCollection selectedItems)
    {
        Name = commandName;
        Parameter = null;
        SelectedItems = selectedItems;
        Cancelled = false;
        IsLoading = false;
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
    public void Cancel(string reason = "Cancelled")
    {
        Cancelled = true;
        CancelReason = reason;
    }

    #endregion
}