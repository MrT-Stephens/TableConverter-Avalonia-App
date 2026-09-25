using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;
using TableConverter.Utilities;
using TableConverter.Utilities.Collections;
using TableConverter.Utilities.Extensions;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.Commands.Services;

public class CommandManager(ILogger<CommandManager> logger) : ICommandManager
{
    #region Fields
    
    /// <summary>
    /// Maximum number of requery passes performed for a single dispatcher tick. Guards against a
    /// <c>CanExecute</c> implementation which mutates the state it depends on and would otherwise requery forever.
    /// </summary>
    private const int MaxRequeryPasses = 4;
    
    private readonly IEventRegistrar _eventRegistrar = new EventRegistrar();
    private readonly List<ICommandHandlerBase> _commandHandlers = [];
    
    /// <summary>
    /// Wrapped in <see cref="Lazy{T}"/> so the side effecting factory (event subscriptions) can only ever run once
    /// per key. <see cref="ConcurrentDictionary{TKey,TValue}.GetOrAdd(TKey,Func{TKey,TValue})"/> may invoke its
    /// factory multiple times under contention, which would leak orphaned subscriptions.
    /// </summary>
    private readonly ConcurrentDictionary<(string, object?), Lazy<ICommandInstance>> _instances = [];
    
    /// <summary>
    /// Coalesces requery requests so a burst of notifications (for example selecting 10,000 rows) results in a
    /// single <c>CanExecute</c> evaluation per command instead of thousands.
    /// </summary>
    private readonly HashSet<IRelayCommand> _pendingRequery = [];
    private readonly object _pendingRequeryLock = new();
    private bool _RequeryScheduled;
    
    private readonly ILogger _logger = logger;
    
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
        var eventId = new EventId(0, nameof(RegisterCommand));
        
        if (_commandHandlers.Any(c => c.CommandMetadata.Name == name))
        {
            _logger.LogError(eventId, "A command with the name '{0}' is already registered.", name);
            
            throw new ArgumentException("A command with the name '{0}' is already registered.".Format(name),
                nameof(name));
        }

        if (handler.CommandMetadata.Name != name)
        {
            _logger.LogError(eventId, "The command handler's name '{0}' does not match the provided name '{1}'.", handler.CommandMetadata.Name, name);
            
            throw new ArgumentException("The command handler's name '{0}' does not match the provided name '{1}'."
                .Format(handler.CommandMetadata.Name, name), nameof(name));
        }

        _commandHandlers.Add(handler);
    }

    public ICommandInstance RegisterCommandInstance(string name, object? viewModel = null)
    {
        var eventId = new EventId(1, nameof(RegisterCommandInstance));
        
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogError(eventId, "Command name cannot be null or whitespace.");
            
            throw new ArgumentException("Command name cannot be null or whitespace.", nameof(name));
        }

        // Lazy guarantees the (side effecting) factory runs exactly once per key, even under concurrent access.
        var instance = _instances.GetOrAdd((name, viewModel), _ => new Lazy<ICommandInstance>(
            () => CreateCommandInstance(name, viewModel, eventId),
            LazyThreadSafetyMode.ExecutionAndPublication));

        return instance.Value;
    }

    private ICommandInstance CreateCommandInstance(string name, object? viewModel, EventId eventId)
    {
        _logger.LogInformation(eventId, "Registering command instance for command '{0}' with view model '{1}'.", name, viewModel?.GetType().Name ?? "null");
        
        if (_commandHandlers.FirstOrDefault(c => c.CommandMetadata.Name == name)
            is not { } handler)
        {
            _logger.LogError(eventId, "Command with name '{0}' not found.", name);
            
            throw new ArgumentException("Command with name '{0}' not found.".Format(name), nameof(name));
        }

        // A view model which has not been initialised yet exposes a null SelectedItems. Fall back to an empty,
        // context-owned collection so the instance can still be created and bound.
        var selection = (viewModel as IHasSelectedItems)?.SelectedItems;

        var context = selection is null
            ? new CommandContext(handler.CommandMetadata.Name)
            : new CommandContext(handler.CommandMetadata.Name, selection);
        
        context.Parent = viewModel;

        IRelayCommand command = handler switch
        {
            ICommandHandlerAsync asyncHandler => new AsyncRelayCommand<object?>(
                param => InternalExecuteAsync(param, asyncHandler, context, handler.CommandMetadata),
                param => InternalCanExecute(param, asyncHandler, context, handler.CommandMetadata),
                // AsyncRelayCommand refuses to run while it is already running unless this flag is set, which would
                // silently override the handler's own AllowConcurrentExecutions metadata.
                handler.CommandMetadata.AllowConcurrentExecutions
                    ? AsyncRelayCommandOptions.AllowConcurrentExecutions
                    : AsyncRelayCommandOptions.None),
            ICommandHandler syncHandler => new RelayCommand<object?>(
                param => InternalExecute(param, syncHandler, context, handler.CommandMetadata),
                param => InternalCanExecute(param, syncHandler, context, handler.CommandMetadata)),
            _ => throw new ArgumentException("Command handler for '{0}' is of an unsupported type.".Format(name),
                nameof(name))
        };

        // Re-evaluate whenever the selection changes. Owned by the command so the subscriptions are released with
        // the command rather than living until the (singleton) manager is disposed.
        _eventRegistrar.RegisterCollectionChanged(context.SelectedItems, command,
            (_, _) => RequestRequery(command));
        
        _eventRegistrar.RegisterEvent<EventHandler<ItemChangedEventArgs>>(
            func => context.SelectedItems.ItemChanged += func,
            func => context.SelectedItems.ItemChanged -= func,
            command, (_, _) => RequestRequery(command));

        // Re-evaluate whenever the owning view model's own state changes (active document/tool, busy flags,
        // search settings, ...). This is what WPF gets for free from CommandManager.RequerySuggested.
        if (viewModel is INotifyPropertyChanged parentNotify)
        {
            _eventRegistrar.RegisterPropertyChanged(parentNotify, command, (_, _) => RequestRequery(command));
        }

        // Re-evaluate when this command starts or finishes processing so IsProcessing is reflected in CanExecute.
        if (context is INotifyPropertyChanged contextNotify)
        {
            _eventRegistrar.RegisterPropertyChanged(contextNotify, command, (_, args) =>
            {
                if (args.PropertyName == nameof(ICommandContext.IsProcessing))
                {
                    RequestRequery(command);
                }
            });
        }

        return new CommandInstance(command, handler, context);
    }
    
    #endregion

    #region Command Retrieval Methods
    
    public ICommandInstance GetCommandInstance(string name, object? viewModel)
    {
        if (_instances.TryGetValue((name, viewModel), out var instance))
        {
            return instance.Value;
        }

        return RegisterCommandInstance(name, viewModel);
    }

    public ICommandInstance this[string name] => GetCommandInstance(name, null);
    
    public ICommandInstance this[string name, object? viewModel] => GetCommandInstance(name, viewModel);

    /// <inheritdoc />
    public void ReleaseCommandInstances(object? viewModel)
    {
        var released = 0;

        // Keys returns a snapshot, so removing while enumerating is safe.
        foreach (var key in _instances.Keys)
        {
            // Reference comparison: the key is the exact instance handed to RegisterCommandInstance, so a value
            // equality override on the view model must never cause an unrelated instance to be released.
            if (!ReferenceEquals(key.Item2, viewModel) || !_instances.TryRemove(key, out var instance))
            {
                continue;
            }

            if (instance.IsValueCreated)
            {
                // Releases the SelectedItems/view model subscriptions owned by this command.
                _eventRegistrar.Clear(instance.Value.RelayCommand);
            }

            released++;
        }

        if (released > 0)
        {
            _logger.LogDebug(new EventId(5, nameof(ReleaseCommandInstances)),
                "Released {0} command instance(s) for view model '{1}'.", released,
                viewModel?.GetType().Name ?? "null");
        }
    }
    
    #endregion

    #region Requery Methods

    /// <inheritdoc />
    public void InvalidateRequerySuggested()
    {
        foreach (var instance in GetInstances())
        {
            RequestRequery(instance.RelayCommand);
        }
    }

    /// <inheritdoc />
    public void InvalidateRequerySuggested(object? viewModel)
    {
        foreach (var instance in GetInstances())
        {
            if (ReferenceEquals(instance.Context.Parent, viewModel))
            {
                RequestRequery(instance.RelayCommand);
            }
        }
    }

    /// <inheritdoc />
    public void InvalidateRequerySuggested(string name, object? viewModel)
    {
        if (_instances.TryGetValue((name, viewModel), out var instance) && instance.IsValueCreated)
        {
            RequestRequery(instance.Value.RelayCommand);
        }
    }

    /// <summary>
    /// Queues a <c>CanExecuteChanged</c> notification for <paramref name="command"/>. Notifications are coalesced
    /// and dispatched once per UI tick so a burst of changes results in a single <c>CanExecute</c> evaluation.
    /// </summary>
    private void RequestRequery(IRelayCommand command)
    {
        lock (_pendingRequeryLock)
        {
            _pendingRequery.Add(command);

            if (_RequeryScheduled)
            {
                return;
            }

            _RequeryScheduled = true;
        }

        Dispatcher.UIThread.Post(DrainRequeryQueue);
    }

    private void DrainRequeryQueue()
    {
        for (var pass = 0; pass < MaxRequeryPasses; pass++)
        {
            IRelayCommand[] commands;

            lock (_pendingRequeryLock)
            {
                commands = [.. _pendingRequery];
                _pendingRequery.Clear();

                if (commands.Length == 0)
                {
                    _RequeryScheduled = false;
                    return;
                }
            }

            foreach (var command in commands)
            {
                command.NotifyCanExecuteChanged();
            }
        }

        // A CanExecute handler kept requesting another pass - release the flag so a later change can schedule one.
        lock (_pendingRequeryLock)
        {
            _RequeryScheduled = false;
        }
    }

    private IEnumerable<ICommandInstance> GetInstances()
    {
        foreach (var instance in _instances.Values)
        {
            if (instance.IsValueCreated)
            {
                yield return instance.Value;
            }
        }
    }
    
    #endregion

    #region Internal Methods
    
    private async Task InternalExecuteAsync(object? parameter, ICommandHandlerAsync handler, ICommandContext context, ICommandMetadata metadata)
    {
        var eventId = new EventId(2, nameof(InternalExecuteAsync));
        
        _logger.LogInformation(eventId, "Executing command '{0}' with parameter '{1}'.", handler.CommandMetadata.Name, parameter ?? "null");
        
        try
        {
            BeginExecution(parameter, context, metadata);

            // Notify subscribers that the command is being executed
            OnExecute?.Invoke(this, context);

            // Execute the command asynchronously
            await handler.Execute(parameter, context);

            // Notify subscribers that the command has been executed
            OnExecuted?.Invoke(this, context);
        }
        catch (Exception e)
        {
            HandleExecutionError(eventId, handler, e);
        }
        finally
        {
            EndExecution(context, metadata);
            
            _logger.LogInformation(eventId, "Finished executing command '{0}'.", handler.CommandMetadata.Name);
        }
    }
    
    private void InternalExecute(object? parameter, ICommandHandler handler, ICommandContext context, ICommandMetadata metadata)
    {
        var eventId = new EventId(3, nameof(InternalExecute));
        
        _logger.LogInformation(eventId, "Executing command '{0}' with parameter '{1}'.", handler.CommandMetadata.Name, parameter ?? "null");
        
        try
        {
            BeginExecution(parameter, context, metadata);

            // Notify subscribers that the command is being executed
            OnExecute?.Invoke(this, context);

            // Execute the command
            handler.Execute(parameter, context);

            // Notify subscribers that the command has been executed
            OnExecuted?.Invoke(this, context);
        }
        catch (Exception e)
        {
            HandleExecutionError(eventId, handler, e);
        }
        finally
        {
            EndExecution(context, metadata);
            
            _logger.LogInformation(eventId, "Finished executing command '{0}'.", handler.CommandMetadata.Name);
        }
    }

    /// <summary>
    /// Applies the pre-execution state shared by the synchronous and asynchronous paths: the public execution
    /// parameter, a reset cancellation state plus the (metadata gated) loading flags. Setting
    /// <see cref="ICommandContext.IsProcessing"/> triggers a requery so bound controls disable themselves while
    /// the handler runs.
    /// </summary>
    private static void BeginExecution(object? parameter, ICommandContext context, ICommandMetadata metadata)
    {
        // Cancellation is per-execution state: without this reset a <c>Cancel</c> from a previous run would still
        // be observed by the next execution (or by anything reading the context afterwards).
        context.Cancelled = false;
        context.CancelReason = string.Empty;

        context.IsProcessing = true;

        if (metadata.CanSetLoadingState)
        {
            context.IsLoading = true;
        }

        if (metadata.CanSetLoadingOnWorkspace && context.Parent is IWorkspace workspace)
        {
            workspace.SetBusy(true);
        }

        // Set the parameter in the context before executing
        context.Parameter = parameter;
    }

    /// <summary>
    /// Reverses <see cref="BeginExecution"/> and clears the per-execution parameter/result so a stale value is
    /// never observed by the next <c>CanExecute</c> evaluation or by a binding.
    /// </summary>
    private static void EndExecution(ICommandContext context, ICommandMetadata metadata)
    {
        context.Parameter = null;
        context.Result = null;
        context.IsProcessing = false;

        if (metadata.CanSetLoadingState)
        {
            context.IsLoading = false;
        }

        if (metadata.CanSetLoadingOnWorkspace && context.Parent is IWorkspace workspace)
        {
            workspace.SetBusy(false);
        }
    }

    private void HandleExecutionError(EventId eventId, ICommandHandlerBase handler, Exception exception)
    {
        _logger.LogError(eventId, exception, "An error occurred while executing the command `{0}`.", handler.CommandMetadata.Name);
        OnError?.Invoke(this, exception);
    }
    
    private bool InternalCanExecute(object? parameter, ICommandHandlerBase handler, ICommandContext context, ICommandMetadata metadata)
    {
        var eventId = new EventId(4, nameof(InternalCanExecute));

        // CanExecute is polled by the UI (per bound control, per frame), so keep this off the information level.
        _logger.LogTrace(eventId, "Checking can execute for command '{0}' with parameter '{1}'.", handler.CommandMetadata.Name, parameter ?? "null");

        // Preserve the parameter of an in-flight execution. The UI polls CanExecute while an async handler is
        // still running, so clearing the parameter in the finally block would clobber the handler's input.
        var inFlightParameter = context.Parameter;

        try
        {
            // Never re-evaluate the parameter of a command which is currently executing - the UI polls
            // CanExecute while an async handler is in flight and would otherwise clobber the in-flight parameter.
            if (!metadata.AllowConcurrentExecutions && context.IsProcessing)
            {
                return false;
            }

            // Set the parameter in the context before checking can execute
            context.Parameter = parameter;

            // Notify subscribers that can execute is being checked
            OnCanExecute?.Invoke(this, context);

            // Return whether the command can execute
            return handler.CanExecute(parameter, context);
        }
        catch (Exception e)
        {
            _logger.LogError(eventId, e, "An error occurred while checking can execute for the command `{0}`.", handler.CommandMetadata.Name);
            OnError?.Invoke(this, e);
            return false;
        }
        finally
        {
            // Restore rather than clear so a handler which is still executing keeps its parameter.
            context.Parameter = inFlightParameter;

            _logger.LogTrace(eventId, "Finished checking can execute for command '{0}'.", handler.CommandMetadata.Name);
        }
    }
    
    #endregion

    #region IDisposable Implementation
    
    public void Dispose()
    {
        _eventRegistrar.ClearAll();
        _commandHandlers.Clear();
        _instances.Clear();
        
        lock (_pendingRequeryLock)
        {
            _pendingRequery.Clear();
            _RequeryScheduled = false;
        }
    }
    
    #endregion
}