using System;
using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;

namespace TableConverter.Commands.Services;

public class CommandManager : ICommandManager
{
    private readonly Dictionary<string, ICommand> _Commands;

    public CommandManager()
    {
        _Commands = new Dictionary<string, ICommand>();
    }

    public event EventHandler<ICommandContext>? OnCanExecute;
    public event EventHandler<ICommandContext>? OnExecute;
    public event EventHandler<ICommandContext>? OnExecuted;

    public void RegisterCommand(string name, ICommandHandler handler)
    {
        var ctx = new CommandContext(name);

        var cmd = new RelayCommand<object>(
            param =>
            {
                // Set the parameter in the context before executing
                ctx.Parameter = param;

                // Notify subscribers that the command is being executed
                OnExecute?.Invoke(this, ctx);
                
                // Execute the command
                handler.Execute(param, ctx);
                
                // Notify subscribers that the command has been executed
                OnExecuted?.Invoke(this, ctx);

                // Clear the parameter and selected items after execution
                ctx.Parameter = null;
                ctx.ClearSelectedItems();
                ctx.Result = null;
            },
            param =>
            {
                // Set the parameter in the context before checking can execute
                ctx.Parameter = param;

                // Notify subscribers that can execute is being checked
                OnCanExecute?.Invoke(this, ctx);
                
                // Return whether the command can execute
                return handler.CanExecute(param, ctx);
            });
        
        _Commands[name] = cmd;
    }

    public void RegisterCommandAsync(string name, ICommandHandlerAsync handler)
    {
        var ctx = new CommandContext(name);
        
        var cmd = new AsyncRelayCommand<object>(
            async param =>
            {
                // Set the parameter in the context before executing
                ctx.Parameter = param;
                
                // Notify subscribers that the command is being executed
                OnExecute?.Invoke(this, ctx);
                
                // Execute the command asynchronously
                await handler.Execute(param, ctx);
                
                // Notify subscribers that the command has been executed
                OnExecuted?.Invoke(this, ctx);
                
                // Clear the parameter and selected items after execution
                ctx.Parameter = null;
                ctx.ClearSelectedItems();
                ctx.Result = null;
            },
            param =>
            {
                // Set the parameter in the context before checking can execute
                ctx.Parameter = param;
                
                // Notify subscribers that can execute is being checked
                OnCanExecute?.Invoke(this, ctx);
                
                // Return whether the command can execute
                return handler.CanExecute(param, ctx);
            });
        
        _Commands[name] = cmd;
    }

    public ICommand GetCommand(string name)
    {
        if (_Commands.TryGetValue(name, out var command))
        {
            return command;
        }

        throw new ArgumentException("No command with the specified name is registered.", nameof(name));
    }

    public ICommand this[string name] => GetCommand(name);
}