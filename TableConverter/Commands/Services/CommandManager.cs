using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;
using TableConverter.Utilities.Extensions;

namespace TableConverter.Commands.Services;

public class CommandManager : ICommandManager
{
    private readonly List<ICommandHandlerBase> _commandHandlers = [];
    private readonly ConcurrentDictionary<(string, object?), ICommandInstance> _instances = new();

    public event EventHandler<ICommandContext>? OnCanExecute;
    public event EventHandler<ICommandContext>? OnExecute;
    public event EventHandler<ICommandContext>? OnExecuted;
    public event EventHandler<Exception>? OnError;

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

            var context = new CommandContext(handler.CommandMetadata.Name);

            ICommand command = handler switch
            {
                ICommandHandlerAsync asyncHandler => new AsyncRelayCommand<object?>(
                    param => InternalExecute(param, asyncHandler, context),
                    param => InternalCanExecute(param, asyncHandler, context)),
                ICommandHandler syncHandler => new RelayCommand<object?>(
                    param => InternalExecuteAsync(param, syncHandler, context),
                    param => InternalCanExecute(param, syncHandler, context)),
                _ => throw new ArgumentException("Command handler for '{0}' is of an unsupported type.".Format(name),
                    nameof(name))
            };

            return new CommandInstance(command, handler, context);
        });

        return instance;
    }
    
    #endregion

    #region Command Retrieval Methods
    
    public ICommandInstance GetCommandInstance(string name, object? viewModel)
    {
        return _instances.TryGetValue((name, viewModel), out var command) 
            ? command 
            : throw new ArgumentException("No command with the specified name is registered.", nameof(name));
    }

    public ICommandInstance this[string name] => GetCommandInstance(name, null);
    
    public ICommandInstance this[string name, object? viewModel] => GetCommandInstance(name, viewModel);
    
    #endregion

    #region Internal Methods
    
    private async Task InternalExecute(object? parameter, ICommandHandlerAsync handler, ICommandContext context)
    {
        try
        {
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
            context.ClearSelectedItems();
            context.Result = null;
        }
    }
    
    private void InternalExecuteAsync(object? parameter, ICommandHandler handler, ICommandContext context)
    {
        try
        {
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
            context.ClearSelectedItems();
            context.Result = null;
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
}