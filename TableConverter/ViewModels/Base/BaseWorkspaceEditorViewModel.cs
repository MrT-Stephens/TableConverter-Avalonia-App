using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TableConverter.Commands.Interfaces;
using TableConverter.Contracts.Events;
using TableConverter.Extensions;
using TableConverter.Interfaces;
using TableConverter.Utilities;
using TableConverter.Utilities.Extensions;
using TableConverter.Utilities.Interfaces;
using TableConverter.ViewModels.Forms;

namespace TableConverter.ViewModels.Base;

public abstract partial class BaseWorkspaceEditorViewModel : BaseViewModel, IWorkspaceEditor, IDisposable
{
    #region Properties
    
    protected readonly IServiceProvider _serviceProvider;
    protected readonly IEventRegistrar _eventRegistrar = new EventRegistrar();

    private readonly IDocumentSession _documentSession;
    private readonly ILogger _logger;

    /// <summary>
    /// Set while documents are being restored so the intermediate states of the restore are never
    /// written back to the session.
    /// </summary>
    private bool _isRestoringDocuments;

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
        _documentSession = serviceProvider.GetRequiredService<IDocumentSession>();
        _logger = serviceProvider.GetRequiredService<ILogger<BaseWorkspaceEditorViewModel>>();
        
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

        // Event subscriptions and command instances live on singleton services, so the removed document has to be
        // released explicitly - otherwise the pane is kept alive for the lifetime of the application.
        if (document is IDisposable disposableDocument)
        {
            disposableDocument.Dispose();
        }

        _toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("Removed")
            .WithContent($"The file '{document!.Title}' has been removed.")
            .Queue();
    }

    #endregion

    #region Abstract Methods

    public abstract IPaneDocument CreateNewDocumentInstance();

    /// <summary>
    /// Creates the document a workspace falls back to when there is nothing to restore. Asynchronous
    /// because creating it can involve reading data from disk.
    /// </summary>
    protected abstract Task<IPaneDocument> CreateDefaultDocumentInstanceAsync();
    
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
        SelectedItems.Remove(oldValue);
        SelectedItems.Add(newValue);
        
        oldValue?.OnDeactivate();
        newValue?.OnActivate();
    }

    #endregion

    #region Virtual Methods

    /// <summary>
    /// The session keys of the document types this workspace is responsible for restoring. It is empty
    /// by default, which keeps a workspace out of the session entirely.
    /// </summary>
    /// <remarks>
    /// A key listed here has to match the <see cref="ISessionDocument.DocumentType" /> of the documents
    /// <see cref="CreateNewDocumentInstance" /> returns, because that is what tells the entries of one
    /// document type apart from another.
    /// </remarks>
    protected virtual IReadOnlyCollection<string> DocumentTypes => [];

    protected virtual void OnDocumentRemoved(IPaneDocument document)
    {
        // Do nothing - Can be overridden
    }

    /// <summary>
    /// Called once the previous session has been restored, so a workspace can clean up whatever the
    /// restored documents leave behind.
    /// </summary>
    protected virtual void OnDocumentsRestored()
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

        // Every change to the open documents is written straight back, so the session always describes
        // what the user can see.
        Documents.CollectionChanged += (_, _) => PersistSession();
    }

    public virtual void InitialiseDocuments()
    {
        if (Documents.Count > 0)
        {
            return;
        }

        RestoreDocumentsAsync().FireAndForget();
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

    public override void Dispose()
    {
        // The workspace outlives every document it opened, so the session gets one last write on the
        // way out.
        PersistSession();

        _eventRegistrar.Dispose();

        base.Dispose();
    }

    #endregion

    #region Session

    /// <summary>
    /// Reopens the documents of this workspace's types that were open last time, falling back to the
    /// workspace's default document when there is nothing to restore. Entries belonging to another
    /// document type are left for the workspace that owns them.
    /// </summary>
    private async Task RestoreDocumentsAsync()
    {
        _isRestoringDocuments = true;

        try
        {
            // A workspace which does not take part in the session has nothing to read.
            IReadOnlyList<DocumentSessionEntry> entries = DocumentTypes.Count == 0
                ? []
                : [.. _documentSession.Load().Where(entry => DocumentTypes.Contains(entry.DocumentType))];

            foreach (var entry in entries)
            {
                if (CreateNewDocumentInstance() is not ISessionDocument document)
                {
                    break;
                }

                try
                {
                    await document.RestoreSessionStateAsync(entry.State);
                }
                catch (Exception exception)
                {
                    // One document that cannot be reopened must not stop the rest from being restored.
                    // It is skipped, and dropped from the session when it is written back.
                    _logger.LogWarning(exception, "A '{DocumentType}' document could not be reopened.",
                        entry.DocumentType);

                    // The document was already resolved from the container, so it has to be released
                    // exactly like a document the user closes would be.
                    if (document is IDisposable disposableDocument)
                    {
                        disposableDocument.Dispose();
                    }

                    continue;
                }

                AddDocument(document);
            }

            if (Documents.Count == 0)
            {
                AddDocument(await CreateDefaultDocumentInstanceAsync());
            }

            OnDocumentsRestored();

            SelectedDocument ??= Documents.FirstOrDefault();
        }
        finally
        {
            _isRestoringDocuments = false;
        }

        PersistSession();
    }

    /// <summary>
    /// Writes the currently open documents to the session, so they can be reopened next time.
    /// </summary>
    private void PersistSession()
    {
        if (_isRestoringDocuments)
        {
            // The intermediate states of a restore are never what the user last saw.
            return;
        }

        foreach (var documentType in DocumentTypes)
        {
            _documentSession.Save(documentType, Documents
                .OfType<ISessionDocument>()
                .Where(document => string.Equals(document.DocumentType, documentType, StringComparison.Ordinal))
                .Select(document => document.CaptureSessionState()));
        }
    }

    #endregion

    #region Misc Methods

    private void AddTools(IEnumerable<IPaneTool> tools)
    {
        foreach (var tool in tools)
        {
            tool.Workspace = this;
            
            if (tool is IInitialise initialiseTool)
            {
                initialiseTool.Initialise();
            }
            
            if (tool is IHasSelectedItems hasSelectedItemsTool)
            {
                hasSelectedItemsTool.SelectedItems = SelectedItems;
            }
            
            Tools.Add(tool);
        }
    }

    #endregion
}