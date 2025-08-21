using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;

namespace TableConverter.ViewModels;

public abstract partial class BasePageViewModel : ObservableValidator
{
    protected readonly ISukiDialogManager DialogManager;
    protected readonly ISukiToastManager ToastManager;

    [ObservableProperty] private string _DisplayName;
    [ObservableProperty] private object _Icon;
    [ObservableProperty] private int _Index;
    [ObservableProperty] private bool _IsLoading;

    public ICommandManager CommandManager { get; }

    protected BasePageViewModel(
        ICommandManager commandManager,
        ISukiDialogManager dialogManager,
        ISukiToastManager toastManager,
        string displayName,
        object? icon,
        int index = 0)
    {
        ArgumentNullException.ThrowIfNull(icon, nameof(icon));

        DialogManager = dialogManager;
        ToastManager = toastManager;
        DisplayName = displayName;
        CommandManager = commandManager;
        Icon = icon;
        Index = index;

        CommandManager.OnCanExecute += OnCommandCanExecute;
        CommandManager.OnExecute += OnCommandExecute;
        CommandManager.OnExecuted += OnCommandExecuted;
    }

    protected virtual void OnCommandExecute(object? sender, ICommandContext context)
    {
        // Do nothing, for override in derived classes if needed
    }
    
    protected virtual void OnCommandExecuted(object? sender, ICommandContext context)
    {
        // Do nothing, for override in derived classes if needed
    }
    
    protected virtual void OnCommandCanExecute(object? sender, ICommandContext context)
    {
        // Do nothing, for override in derived classes if needed
    }
}