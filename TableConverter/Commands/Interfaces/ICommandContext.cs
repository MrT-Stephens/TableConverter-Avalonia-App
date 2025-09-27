using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TableConverter.Utilities;

namespace TableConverter.Commands.Interfaces;

public interface ICommandContext
{
    /// <summary>
    /// Name of the command.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The parameter passed to the command, if any.
    /// </summary>
    public object? Parameter { get; set; }
    
    /// <summary>
    /// The view model associated with the command context, if any.
    /// </summary>
    public object? ViewModel { get; set; }

    /// <summary>
    /// The result of the command execution, if any.
    /// </summary>
    public Result<object>? Result { get; set; }

    /// <summary>
    /// The selected items in the context of the command.
    /// </summary>
    public Dictionary<Type, object> SelectedItems { get; }

    /// <summary>
    /// Updates the selected items in the context of the command with the provided object.
    /// </summary>
    /// <param name="obj">
    /// The object to update the selected items with.
    /// </param>
    /// <typeparam name="TObjectType">
    /// The type of the object to update the selected items with.
    /// </typeparam>
    public void UpdateSelectedItems<TObjectType>(TObjectType obj);

    /// <summary>
    /// Checks if the context has a selected item of the specified type.
    /// </summary>
    /// <typeparam name="TObjectType">
    /// The type of the object to check for in the selected items.
    /// </typeparam>
    /// <returns>
    /// True if there is at least one selected item of the specified type; otherwise, false.
    /// </returns>
    public bool HasSelectedItem<TObjectType>();

    /// <summary>
    /// Attempts to get the selected item of the specified type from the context.
    /// </summary>
    /// <param name="item">
    /// The output parameter that will hold the selected item if found.
    /// </param>
    /// <typeparam name="TObjectType">
    /// The type of the object to retrieve from the selected items.
    /// </typeparam>
    /// <returns>
    /// True if a selected item of the specified type is found; otherwise, false.
    /// </returns>
    public bool TryGetSelectedItem<TObjectType>([NotNullWhen(true)] out TObjectType? item);

    /// <summary>
    /// Clears the selected item of the specified type from the context.
    /// </summary>
    /// <typeparam name="TObjectType">
    /// The type of the object to clear from the selected items.
    /// </typeparam>
    public void ClearSelectedItem<TObjectType>();

    /// <summary>
    /// Clears all selected items from the context.
    /// </summary>
    public void ClearSelectedItems();

    /// <summary>
    /// Sets the result of the command execution to the provided object, wrapped in a successful Result.
    /// </summary>
    /// <param name="obj">
    /// The object to set as the result of the command execution.
    /// </param>
    /// <typeparam name="TObjectType">
    /// The type of the object to set as the result of the command execution.
    /// </typeparam>
    public void SetResult<TObjectType>(TObjectType obj);
    
    /// <summary>
    /// Sets the result of the command execution to an error state with the provided error message.
    /// </summary>
    /// <param name="errorMessage">
    /// The error message describing the reason for the failure.
    /// </param>
    public void SetErrorResult(string errorMessage);

    /// <summary>
    /// Attempts to get the result of the command execution as a specific type.
    /// </summary>
    /// <param name="result">
    /// The output parameter that will hold the result if it is of the specified type.
    /// </param>
    /// <typeparam name="TObjectType">
    /// The type of the result to retrieve from the command execution.
    /// </typeparam>
    /// <returns>
    /// True if the result is successfully retrieved and is of the specified type; otherwise, false.
    /// </returns>
    public bool TryGetResult<TObjectType>([NotNullWhen(true)] out Result<TObjectType>? result);
}