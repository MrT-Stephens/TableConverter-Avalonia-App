using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;

namespace TableConverter.Commands.Services;

public class CommandManager : ICommandManager
{
    private readonly ConcurrentDictionary<(string, object?), ICommand> _commands;
    private readonly ConcurrentDictionary<(string, object?), ICommandContext> _contexts;

    public CommandManager()
    {
        _commands = new ConcurrentDictionary<(string, object?), ICommand>();
        _contexts = new ConcurrentDictionary<(string, object?), ICommandContext>();
    }

    public event EventHandler<ICommandContext>? OnCanExecute;
    public event EventHandler<ICommandContext>? OnExecute;
    public event EventHandler<ICommandContext>? OnExecuted;
    public event EventHandler<Exception>? OnError;

    public void RegisterCommand(string name, ICommandHandler handler)
    {
        var cmd = new RelayCommand<object>(
            param =>
            {
                var ctx = _contexts.GetOrAdd((name, null), new CommandContext(name));

                try
                {
                    // Set the parameter in the context before executing
                    ctx.Parameter = param;

                    // Notify subscribers that the command is being executed
                    OnExecute?.Invoke(this, ctx);

                    // Execute the command
                    handler.Execute(param, ctx);

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
            },
            param =>
            {
                var ctx = _contexts.GetOrAdd((name, null), new CommandContext(name));
                
                try
                {
                    // Set the parameter in the context before checking can execute
                    ctx.Parameter = param;

                    // Notify subscribers that can execute is being checked
                    OnCanExecute?.Invoke(this, ctx);
                    
                    // Return whether the command can execute
                    return handler.CanExecute(param, ctx);
                }
                catch (Exception e)
                {
                    OnError?.Invoke(this, e);
                    return false;
                }
            });
        
        _commands[(name, null)] = cmd;
    }

    public void RegisterCommandAsync(string name, ICommandHandlerAsync handler)
    {
        var cmd = new AsyncRelayCommand<object>(
            async param =>
            {
                var ctx = _contexts.GetOrAdd((name, null), new CommandContext(name));

                try
                {
                    // Set the parameter in the context before executing
                    ctx.Parameter = param;

                    // Notify subscribers that the command is being executed
                    OnExecute?.Invoke(this, ctx);

                    // Execute the command asynchronously
                    await handler.Execute(param, ctx);

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
            },
            param =>
            {
                var ctx = _contexts.GetOrAdd((name, null), new CommandContext(name));
                
                try
                {
                    // Set the parameter in the context before checking can execute
                    ctx.Parameter = param;
                    
                    // Notify subscribers that can execute is being checked
                    OnCanExecute?.Invoke(this, ctx);
                    
                    // Return whether the command can execute
                    return handler.CanExecute(param, ctx);
                }
                catch (Exception e)
                {
                    OnError?.Invoke(this, e);
                    return false;
                }
            });
        
        _commands[(name, null)] = cmd;
    }

    public ICommand GetCommand(string name, object? viewModel)
    {
        return _commands.TryGetValue((name, viewModel), out var command) 
            ? command 
            : throw new ArgumentException("No command with the specified name is registered.", nameof(name));
    }

    public ICommand this[string name] => GetCommand(name, null);
    
    public ICommand this[string name, object? viewModel] => GetCommand(name, viewModel);
}