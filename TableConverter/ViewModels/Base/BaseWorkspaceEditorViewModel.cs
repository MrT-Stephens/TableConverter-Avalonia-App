using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Commands.Interfaces;
using TableConverter.Contracts.Events;
using TableConverter.Extensions;
using TableConverter.Interfaces;
using TableConverter.ViewModels.Forms;

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
    [ObservableProperty] private bool _IsBusy;
    [ObservableProperty] private string _BusyText;
    [ObservableProperty] private IPaneDocument? _SelectedDocument;
    [ObservableProperty] private IPaneTool? _SelectedTool;
    [ObservableProperty] private ToolsSettingsForm _ToolsSettings;

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
        BusyText = string.Empty;
        ToolsSettings = new ToolsSettingsForm(Dock.Right, 350);
        
        Initialise();
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task ToolSettingsButtonClicked(object? parameter)
    {
        await _dialogManager.CreateDialog()
            .WithTitle("Tool Pane Settings")
            .WithForm(ToolsSettings)
            .WithOkResult("Ok")
            .Dismiss()
            .ByClickingBackground()
            .TryShowAsync(CancellationToken.None);
    }

    #endregion

    #region Abstract Methods

    public abstract IPaneDocument CreateNewDocumentInstance();

    protected abstract IPaneDocument CreateDefaultDocumentInstance();
    
    #endregion

    #region Overrides

    partial void OnSelectedDocumentChanged(IPaneDocument? oldValue, IPaneDocument? newValue)
    {
        if (oldValue is not null)
        {
            oldValue.OnDeactivate();
        }

        if (newValue is not null)
        {
            newValue.OnActivate();
        }
        
        _eventManager.GetEvent<WorkspaceDocumentSelectedEvent>()
            .Publish(new WorkspaceDocumentSelectedEventArgs
            {
                Workspace = this,
                OldDocument = oldValue,
                NewDocument = newValue,
            });
    }

    partial void OnSelectedToolChanged(IPaneTool? oldValue, IPaneTool? newValue)
    {
        if (oldValue is not null)
        {
            oldValue.OnDeactivate();
        }

        if (newValue is not null)
        {
            newValue.OnActivate();
        }
    }

    #endregion

    #region Methods

    public void Initialise()
    {
        InitialiseEvents();
        InitialiseTools();
        InitialiseDocuments();
    }

    public void InitialiseDocuments()
    {
        if (Documents.Count > 0)
            return;
        
        var defaultDocument = CreateDefaultDocumentInstance();
        Documents.Add(defaultDocument);
        SelectedDocument = defaultDocument;
    }
    
    public void InitialiseEvents()
    {
        
    }

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

    #endregion
}