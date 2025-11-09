using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;
using TableConverter.Utilities.Extensions;

namespace TableConverter.ViewModels.Base;

public abstract partial class BaseWorkspaceEditorViewModel : BaseViewModel, IWorkspaceEditor
{
    #region Properties
    
    protected readonly IServiceProvider _serviceProvider;

    [ObservableProperty] private string _Title;
    [ObservableProperty] private object _Icon;
    [ObservableProperty] private int _Index;
    [ObservableProperty] private ObservableCollection<IPaneDocument> _Documents;
    [ObservableProperty] private ObservableCollection<IPaneTool> _Tools;
    [ObservableProperty] private Dock _ToolsPosition;
    [ObservableProperty] private bool _IsBusy;
    [ObservableProperty] private string _BusyText;
    [ObservableProperty] private IPaneDocument? _SelectedDocument;
    [ObservableProperty] private IPaneTool? _SelectedTool;

    #endregion

    #region Constructors

    protected BaseWorkspaceEditorViewModel(
        IServiceProvider serviceProvider,
        string title,
        string iconPath,
        int index = 0)
        : base(serviceProvider.GetRequiredService<ICommandManager>(),
            serviceProvider.GetRequiredService<IEventManager>(),
            serviceProvider.GetRequiredService<ISukiDialogManager>(),
            serviceProvider.GetRequiredService<ISukiToastManager>())
    {
        _serviceProvider = serviceProvider;
        
        Title = title;
        Index = index;
        Icon = Application.Current!.Resources[iconPath]
            ?? throw new ArgumentNullException(nameof(iconPath), $"Icon resource '{iconPath}' not found.");
        Documents = [];
        Tools = [];
        ToolsPosition = Dock.Right;
        BusyText = string.Empty;
        
        InitialiseTools();
    }

    #endregion

    #region Methods

    public void InitialiseTools()
    {
        var globalTools = _serviceProvider.GetRequiredService<IEnumerable<IPaneTool>>();
        
        foreach (var tool in globalTools)
        {
            tool.Workspace = this;
            Tools.Add(tool);
        }
        
        var scopedToolsType = typeof(IScopedPaneTool<>).MakeGenericType(GetType());
        var enumerableScopedToolsType = typeof(IEnumerable<>).MakeGenericType(scopedToolsType);
        var scopedTools = _serviceProvider.GetRequiredService(enumerableScopedToolsType);
        
        if (scopedTools is not IEnumerable<IPaneTool> scopedToolsEnumerable)
            return;
        
        foreach (var tool in scopedToolsEnumerable)
        {
            tool.Workspace = this;
            Tools.Add(tool);
        }
    }

    public void SetBusy(bool isBusy, string busyText = "Loading...")
    {
        IsBusy = isBusy;
        BusyText = busyText;
    }
    
    public void ClearBusy()
    {
        IsBusy = false;
        BusyText = string.Empty;
    }

    public void AddNewDocument<TViewModel>(string title, Action<TViewModel>? initializeAction = null)
        where TViewModel : IPaneDocument
    {
        var document = _serviceProvider.GetRequiredService<TViewModel>();
        document.Title = title;
        initializeAction?.Invoke(document);
        Documents.Add(document);
        SelectedDocument = document;
    }

    #endregion
}