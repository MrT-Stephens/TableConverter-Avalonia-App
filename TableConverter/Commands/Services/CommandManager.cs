using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;
using TableConverter.Utilities;
using TableConverter.Utilities.Collections;
using TableConverter.Utilities.Extensions;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Commands.Services;

public class CommandManager : ICommandManager
{
    #region Fields
    
    private readonly IEventRegistrar _eventRegistrar = new EventRegistrar();
    private readonly List<ICommandHandlerBase> _commandHandlers = [];
    private readonly ConcurrentDictionary<(string, object?), ICommandInstance> _instances = [];
    
    #endregion

    #region Events
    
    public event EventHandler<ICommandContext>? OnCanExecute;
    public event EventHandler<ICommandContext>? OnExecute;
    public event EventHandler<ICommandContext>? OnExecuted;
    public event EventHandler<Exception>? OnError;
    
    #endregion

    #region Command Registration Methods

    public void RegisterCommand(string name, ICommandHandlerBase handler)
    {
#if DEBUG
        if (_commandHandlers.Any(c => c.CommandMetadata.Name == name))
            throw new ArgumentException("A command with the name '{0}' is already registered.".Format(name), nameof(name));

        if (handler.CommandMetadata.Name != name)
            throw new ArgumentException("The command handler's name '{0}' does not match the provided name '{1}'."
                .Format(handler.CommandMetadata.Name, name), nameof(name));
#endif

        _commandHandlers.Add(handler);
    }

    public ICommandInstance RegisterCommandInstance(string name, object? viewModel = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Command name cannot be null or whitespace.", nameof(name));

        var instance = _instances.GetOrAdd((name, viewModel), _ =>
        {
            if (_commandHandlers.FirstOrDefault(c => c.CommandMetadata.Name == name)
                is not { } handler)
            {
                throw new ArgumentException("Command with name '{0}' not found.".Format(name), nameof(name));
            }

            var context = viewModel is IHasSelectedItems hasSelectedItems
                ? new CommandContext(handler.CommandMetadata.Name, hasSelectedItems.SelectedItems)
                : new CommandContext(handler.CommandMetadata.Name);
            
            context.Parent = viewModel;

            IRelayCommand command = handler switch
            {
                ICommandHandlerAsync asyncHandler => new AsyncRelayCommand<object?>(
                    param => InternalExecute(param, asyncHandler, context, handler.CommandMetadata),
                    param => InternalCanExecute(param, asyncHandler, context)),
                ICommandHandler syncHandler => new RelayCommand<object?>(
                    param => InternalExecuteAsync(param, syncHandler, context, handler.CommandMetadata),
                    param => InternalCanExecute(param, syncHandler, context)),
                _ => throw new ArgumentException("Command handler for '{0}' is of an unsupported type.".Format(name),
                    nameof(name))
            };

            _eventRegistrar.RegisterCollectionChanged(context.SelectedItems, null,
                (_, _) =>
                {
                    Dispatcher.UIThread.InvokeAsync(() => command.NotifyCanExecuteChanged());
                });
            
            _eventRegistrar.RegisterEvent<EventHandler<ItemChangedEventArgs>>(
                func => context.SelectedItems.ItemChanged += func,
                func => context.SelectedItems.ItemChanged -= func,
                null, (_, _) =>
                {
                    Dispatcher.UIThread.InvokeAsync(() => command.NotifyCanExecuteChanged());
                });

            return new CommandInstance(command, handler, context);
        });

        return instance;
    }
    
    #endregion

    #region Command Retrieval Methods
    
    public ICommandInstance GetCommandInstance(string name, object? viewModel)
    {
        if (!_instances.TryGetValue((name, viewModel), out var command))
        {
            command = RegisterCommandInstance(name, viewModel);
        }

        return command;
    }

    public ICommandInstance this[string name] => GetCommandInstance(name, null);
    
    public ICommandInstance this[string name, object? viewModel] => GetCommandInstance(name, viewModel);
    
    #endregion

    #region Internal Methods
    
    private async Task InternalExecute(object? parameter, ICommandHandlerAsync handler, ICommandContext context, ICommandMetadata metadata)
    {
        try
        {
            context.IsProcessing = true;
            
            if (metadata.CanSetLoadingState)
            {
                context.IsLoading = true;
            }
            
            // Set the parameter in the context before executing
            context.Parameter = parameter;

            // Notify subscribers that the command is being executed
            OnExecute?.Invoke(this, context);

            // Execute the command asynchronously
            await handler.Execute(parameter, context);

            // Notify subscribers that the command has been executed
            OnExecuted?.Invoke(this, context);
        }
        catch (Exception e)
        {
            OnError?.Invoke(this, e);
        }
        finally
        {
            // Clear the parameter and selected items after execution
            context.Parameter = null;
            context.Result = null;
            context.IsProcessing = false;
            
            if (metadata.CanSetLoadingState)
            {
                context.IsLoading = false;
            }
        }
    }
    
    private void InternalExecuteAsync(object? parameter, ICommandHandler handler, ICommandContext context, ICommandMetadata metadata)
    {
        try
        {
            context.IsProcessing = true;
            
            if (metadata.CanSetLoadingState)
            {
                context.IsLoading = true;
            }
            
            // Set the parameter in the context before executing
            context.Parameter = parameter;

            // Notify subscribers that the command is being executed
            OnExecute?.Invoke(this, context);

            // Execute the command asynchronously
            handler.Execute(parameter, context);

            // Notify subscribers that the command has been executed
            OnExecuted?.Invoke(this, context);
        }
        catch (Exception e)
        {
            OnError?.Invoke(this, e);
        }
        finally
        {
            // Clear the parameter and selected items after execution
            context.Parameter = null;
            context.Result = null;
            context.IsProcessing = false;
            
            if (metadata.CanSetLoadingState)
            {
                context.IsLoading = false;
            }
        }
    }
    
    private bool InternalCanExecute(object? parameter, ICommandHandlerBase handler, ICommandContext context)
    {
        try
        {
            // Set the parameter in the context before checking can execute
            context.Parameter = parameter;
                    
            // Notify subscribers that can execute is being checked
            OnCanExecute?.Invoke(this, context);
                    
            // Return whether the command can execute
            return handler.CanExecute(parameter, context);
        }
        catch (Exception e)
        {
            OnError?.Invoke(this, e);
            return false;
        }
    }
    
    #endregion

    #region IDisposable Implementation
    
    public void Dispose()
    {
        _eventRegistrar.ClearAll();
        _commandHandlers.Clear();
        _instances.Clear();
    }
    
    #endregion
}