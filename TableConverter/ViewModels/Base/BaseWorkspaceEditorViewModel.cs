using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Commands.Interfaces;
using TableConverter.Contracts.Events;
using TableConverter.Extensions;
using TableConverter.Interfaces;
using TableConverter.Services;
using TableConverter.Utilities.Extensions;
using TableConverter.ViewModels.Forms;

namespace TableConverter.ViewModels.Base;

public abstract partial class BaseWorkspaceEditorViewModel : BaseViewModel, IWorkspaceEditor, IDisposable
{
    #region Properties
    
    protected readonly IServiceProvider _serviceProvider;
    protected readonly IEventRegistrar _eventRegistrar = new EventRegistrar();

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
    [ObservableProperty] private ObservableCollection<ICommandInstance> _MainCommands;
    
    protected readonly string _storageLocationPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
        "TableConverter");

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
        MainCommands = [];
        BusyText = string.Empty;
        ToolsSettings = new ToolsSettingsForm(Dock.Right, 350);
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

    [RelayCommand]
    private async Task RemoveFileButtonClicked(object? parameter)
    {
        if (Documents.Count == 0 
            || parameter is not IPaneDocument { CanClose: true } document)
        {
            return;
        }

        if (!await _dialogManager.CreateDialog()
                .WithTitle("Are you sure?")
                .WithContent($"This will remove the file '{document!.Title}'.")
                .WithYesNoResult("Yes", "No")
                .TryShowAsync())
        {
            return;
        }
        
        SelectedDocument = null;
        Documents.Remove(document);
        OnDocumentRemoved(document);

        _toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("Removed")
            .WithContent($"The file '{document!.Title}' has been removed.")
            .Queue();
    }

    #endregion

    #region Abstract Methods

    public abstract IPaneDocument CreateNewDocumentInstance();

    protected abstract IPaneDocument CreateDefaultDocumentInstance();
    
    #endregion

    #region Overrides

    partial void OnSelectedDocumentChanged(IPaneDocument? oldValue, IPaneDocument? newValue)
    {
        oldValue?.OnDeactivate();
        newValue?.OnActivate();
        
        SelectedItems.Remove(oldValue);
        SelectedItems.Add(newValue);

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
        oldValue?.OnDeactivate();
        newValue?.OnActivate();

        SelectedItems.Remove(oldValue);
        SelectedItems.Add(newValue);
    }

    #endregion

    #region Virtual Methods

    protected virtual void OnDocumentRemoved(IPaneDocument document)
    {
        // Do nothing - Can be overridden
    }

    #endregion

    #region Methods

    public override void Initialise()
    {
        base.Initialise();
        
        InitialiseEvents();
        InitialiseTools();
        InitialiseDocuments();
    }

    public void InitialiseDocuments()
    {
        if (Documents.Count > 0)
            return;

        AddDocument(CreateDefaultDocumentInstance());
    }
    
    public void InitialiseEvents()
    {
        
    }

    public void InitialiseTools()
    {
        var globalTools = _serviceProvider.GetRequiredService<IEnumerable<IPaneTool>>();
        
        AddTools(globalTools);
        
        var scopedToolsType = typeof(IScopedPaneTool<>).MakeGenericType(GetType());
        var enumerableScopedToolsType = typeof(IEnumerable<>).MakeGenericType(scopedToolsType);
        var scopedTools = _serviceProvider.GetRequiredService(enumerableScopedToolsType);
        
        if (scopedTools is not IEnumerable<IPaneTool> scopedToolsEnumerable)
            return;
        
        AddTools(scopedToolsEnumerable);
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

    public void AddDocument(IPaneDocument document)
    {
        document.Workspace = this;
        
        if (document is IHasSelectedItems hasSelectedItemsDocument)
        {
            hasSelectedItemsDocument.SelectedItems = SelectedItems;
        }
        
        Documents.Add(document);
    }
    
    public void ShowTool<T>() where T : IPaneTool
    {
        var tool = Tools.GetSingleOfType<T>();
        SelectedTool = tool;
    }

    public void Dispose()
    {
        _eventRegistrar.Dispose();
    }

    #endregion

    #region Misc Methods

    private void AddTools(IEnumerable<IPaneTool> tools)
    {
        foreach (var tool in tools)
        {
            tool.Workspace = this;
            
            if (tool is IHasSelectedItems hasSelectedItemsTool)
            {
                hasSelectedItemsTool.SelectedItems = SelectedItems;
            }

            if (tool is IInitialise initialiseTool)
            {
                initialiseTool.Initialise();
            }
            
            Tools.Add(tool);
        }
    }

    #endregion
}