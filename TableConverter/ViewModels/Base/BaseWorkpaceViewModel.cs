using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;
using TableConverter.ViewModels.Base;

namespace TableConverter.ViewModels;

public partial class BaseWorkpaceViewModel : BaseViewModel, IWorkspace
{
    #region Properties

    [ObservableProperty] private string _Title;
    [ObservableProperty] private object _Icon;
    [ObservableProperty] private int _Index;

    #endregion

    #region Constructors

    public BaseWorkpaceViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager, 
        ISukiToastManager toastManager,
        string title,
        string iconPath,
        int index = 0) 
        : base(commandManager, eventManager, dialogManager, toastManager)
    {
        Title = title;
        Index = index;

        Icon = Application.Current!.Resources[iconPath]
            ?? throw new ArgumentNullException(nameof(iconPath), $"Icon resource '{iconPath}' not found.");
    }

    #endregion
}

