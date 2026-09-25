using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Common;
using TableConverter.Interfaces;
using TableConverter.Utilities.Collections;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Utilities.Extensions;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.ViewModels.Base;

public abstract partial class BaseViewModel : ObservableValidator, IInitialise, IHasSelectedItems, IDisposable
{
    #region Fields

    protected readonly ICommandManager _commandManager;
    protected readonly IEventManager _eventManager;
    protected readonly ISukiDialogManager _dialogManager;
    protected readonly ISukiToastManager _toastManager;

    private bool _disposed;

    [ObservableProperty] private SelectedItemsCollection _SelectedItems = null!;

    #endregion

    #region Constructors

    public BaseViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager,
        ISukiDialogManager dialogManager,
        ISukiToastManager  toastManager)
    {
        _commandManager = commandManager;
        _eventManager = eventManager;
        _dialogManager = dialogManager;
        _toastManager = toastManager;

        _commandManager.OnCanExecute += OnCanExecuteCommand;
        _commandManager.OnExecute += OnExecuteCommand;
        _commandManager.OnExecuted += OnExecutedCommand;
        _commandManager.OnError += OnErrorCommand;
    }

    #endregion

    #region IDisposable

    /// <summary>
    ///     Gets a value indicating whether this view model has been disposed.
    /// </summary>
    protected bool IsDisposed => _disposed;

    /// <summary>
    ///     Releases the subscriptions this view model made against long-lived services. Derived classes that override
    ///     this method must call <c>base.Dispose()</c>.
    /// </summary>
    public virtual void Dispose()
    {
        if (_disposed) return;

        _disposed = true;

        // ICommandManager is a singleton, so these subscriptions would otherwise keep every view model alive.
        _commandManager.OnCanExecute -= OnCanExecuteCommand;
        _commandManager.OnExecute -= OnExecuteCommand;
        _commandManager.OnExecuted -= OnExecutedCommand;
        _commandManager.OnError -= OnErrorCommand;

        // Release the command instances registered against this view model (and the subscriptions they own).
        // Without this the singleton manager would keep both the instances and this view model alive forever.
        _commandManager.ReleaseCommandInstances(this);

        _eventManager.UnregisterAllEvents(this);
    }

    #endregion

    #region Command Handler Events

    protected virtual void OnCanExecuteCommand(object? sender, ICommandContext context)
    {
        // Override in derived classes if needed
    }

    protected virtual void OnExecuteCommand(object? sender, ICommandContext context)
    {
        // Override in derived classes if needed
    }
    
    protected virtual void OnExecutedCommand(object? sender, ICommandContext context)
    {
        // Override in derived classes if needed
    }
    
    protected virtual void OnErrorCommand(object? sender, Exception exception)
    {
        // Override in derived classes if needed
    }

    #endregion

    #region Misc

    public virtual ICommandInstance this[string commandName] => _commandManager[commandName, this];

    public virtual void Initialise()
    {
        SelectedItems = [];
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
}