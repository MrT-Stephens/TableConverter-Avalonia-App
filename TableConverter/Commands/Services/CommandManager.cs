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
    private readonly List<ICommandHandlerBase> _commandHandlers;
    private readonly ConcurrentDictionary<(string, object?), ICommand> _commands;
    private readonly ConcurrentDictionary<(string, object?), ICommandContext> _contexts;

    public CommandManager()
    {
        _commandHandlers = new List<ICommandHandlerBase>();
        _commands = new ConcurrentDictionary<(string, object?), ICommand>();
        _contexts = new ConcurrentDictionary<(string, object?), ICommandContext>();
    }

    public event EventHandler<ICommandContext>? OnCanExecute;
    public event EventHandler<ICommandContext>? OnExecute;
    public event EventHandler<ICommandContext>? OnExecuted;
    public event EventHandler<Exception>? OnError;

    #region Command Registration Methods

    public void RegisterCommand(string name, ICommandHandlerBase handler)
    {
        if (_commandHandlers.Any(c => c.CommandMetadata.Name == name))
            throw new ArgumentException("A command with the name '{0}' is already registered.".Format(name), nameof(name));

        if (handler.CommandMetadata.Name != name)
            throw new ArgumentException("The command handler's name '{0}' does not match the provided name '{1}'."
                .Format(handler.CommandMetadata.Name, name), nameof(name));

        _commandHandlers.Add(handler);
    }

    public void RegisterCommandInstance(string name, object? viewModel = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Command name cannot be null or whitespace.", nameof(name));

        var cmd = _commands.GetOrAdd((name, viewModel), _ =>
        {
            if (_commandHandlers.FirstOrDefault(c => c.CommandMetadata.Name == name)
                is not { } handler)
            {
                throw new ArgumentException("Command with name '{0}' not found.".Format(name), nameof(name));
            }

            return handler switch
            {
                ICommandHandlerAsync asyncHandler => new AsyncRelayCommand<object?>(
                    param => InternalExecute((name, viewModel), param, asyncHandler),
                    param => InternalCanExecute((name, viewModel), param, asyncHandler)),
                ICommandHandler syncHandler => new RelayCommand<object?>(
                    param => InternalExecuteAsync((name, viewModel), param, syncHandler),
                    param => InternalCanExecute((name, viewModel), param, syncHandler)),
                _ => throw new ArgumentException(
                    "Command handler for '{0}' is of an unsupported type."
                    .Format(name), nameof(name))
            };
        });
        
        _commands[(name, viewModel)] = cmd;
    }
    
    #endregion

    #region Command Retrieval Methods
    
    public ICommand GetCommand(string name, object? viewModel)
    {
        return _commands.TryGetValue((name, viewModel), out var command) 
            ? command 
            : throw new ArgumentException("No command with the specified name is registered.", nameof(name));
    }

    public ICommand this[string name] => GetCommand(name, null);
    
    public ICommand this[string name, object? viewModel] => GetCommand(name, viewModel);
    
    #endregion

    #region Internal Methods
    
    private async Task InternalExecute((string, object?) key, object? parameter, ICommandHandlerAsync command)
    {
        var ctx = _contexts.GetOrAdd(key, new CommandContext(command.CommandMetadata.Name));
        
        try
        {
            // Set the parameter in the context before executing
            ctx.Parameter = parameter;

            // Notify subscribers that the command is being executed
            OnExecute?.Invoke(this, ctx);

            // Execute the command asynchronously
            await command.Execute(parameter, ctx);

            // Notify subscribers that the command has been executed
            OnExecuted?.Invoke(this, ctx);
        }
        catch (Exception e)
        {
            OnError?.Invoke(this, e);
        }
        finally
        {
            // Clear the parameter and selected items after execution
            ctx.Parameter = null;
            ctx.ClearSelectedItems();
            ctx.Result = null;
        }
    }
    
    private void InternalExecuteAsync((string, object?) key, object? parameter, ICommandHandler command)
    {
        var ctx = _contexts.GetOrAdd(key, new CommandContext(command.CommandMetadata.Name));
        
        try
        {
            // Set the parameter in the context before executing
            ctx.Parameter = parameter;

            // Notify subscribers that the command is being executed
            OnExecute?.Invoke(this, ctx);

            // Execute the command asynchronously
            command.Execute(parameter, ctx);

            // Notify subscribers that the command has been executed
            OnExecuted?.Invoke(this, ctx);
        }
        catch (Exception e)
        {
            OnError?.Invoke(this, e);
        }
        finally
        {
            // Clear the parameter and selected items after execution
            ctx.Parameter = null;
            ctx.ClearSelectedItems();
            ctx.Result = null;
        }
    }
    
    private bool InternalCanExecute((string, object?) key, object? parameter, ICommandHandlerBase command)
    {
        var ctx = _contexts.GetOrAdd(key, new CommandContext(command.CommandMetadata.Name));
                
        try
        {
            // Set the parameter in the context before checking can execute
            ctx.Parameter = parameter;
                    
            // Notify subscribers that can execute is being checked
            OnCanExecute?.Invoke(this, ctx);
                    
            // Return whether the command can execute
            return command.CanExecute(parameter, ctx);
        }
        catch (Exception e)
        {
            OnError?.Invoke(this, e);
            return false;
        }
    }
    
    #endregion
}