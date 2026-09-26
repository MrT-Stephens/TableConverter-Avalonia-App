using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Handlers.TableData;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;
using TableConverter.Services.DataSources;
using TableConverter.Utilities.Interfaces;
using TableConverter.ViewModels.Base;
using TableConverter.ViewModels.Documents;
using TableConverter.ViewModels.Workspaces;

namespace TableConverter.ViewModels.Tools;

public partial class TableUtilitiesViewModel : BaseScopedPaneToolViewModel<TableWorkspaceEditorViewModel>
{
    #region Properties

    [ObservableProperty] private int _HeadersCount;
    [ObservableProperty] private int _RowCount;
    [ObservableProperty] private ObservableCollection<ICommandInstance> _GeneralCommands;

    /// <summary>
    ///     The data source of the document currently shown in this tool, or null when the selected document is not a
    ///     table. Held so the row/column change notifications can be released again.
    /// </summary>
    private TableStoreDataSource? _DataSource;

    private INotifyCollectionChanged? _RowCollectionChangedSource;

    #endregion
    
    #region Constructors
    
    public TableUtilitiesViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager, 
        ISukiToastManager toastManager) 
        : base(commandManager, eventManager, dialogManager, toastManager, "Table Utilities")
    {
        HeadersCount = 0;
        RowCount = 0;
        GeneralCommands = [];
    }
    
    #endregion

    #region Overrides

    public override void Initialise()
    {
        base.Initialise();

        // Each table tool is a command, so the same buttons serve the tool pane and the workspace menu, and
        // there is one implementation behind both.
        GeneralCommands.Add(this[TableDataCommandNames.AddRow]);
        GeneralCommands.Add(this[TableDataCommandNames.DeleteRows]);
        GeneralCommands.Add(this[TableDataCommandNames.TrimWhitespace]);
        GeneralCommands.Add(this[TableDataCommandNames.RemoveDuplicateRows]);
        GeneralCommands.Add(this[TableDataCommandNames.TransposeClockwise]);
        GeneralCommands.Add(this[TableDataCommandNames.TransposeCounterClockwise]);
        GeneralCommands.Add(this[TableDataCommandNames.SortByColumn]);
    }

    protected override void OnSelectedDocumentChanged(IWorkspace workspace, 
        IPaneDocument? oldDocument, IPaneDocument? newDocument)
    {
        base.OnSelectedDocumentChanged(workspace, oldDocument, newDocument);

        if (Workspace != workspace)
        {
            return;
        }

        DetachDataSource();

        if (newDocument is not TableDataViewModel tableDataViewModel)
        {
            UpdateCounts();
            return;
        }

        _DataSource = tableDataViewModel.DataSource;
        _DataSource.PropertyChanged += OnDataSourcePropertyChanged;

        _RowCollectionChangedSource = _DataSource.Collection;
        _RowCollectionChangedSource.CollectionChanged += OnRowCollectionChanged;

        UpdateCounts();
    }

    public override void Dispose()
    {
        DetachDataSource();

        base.Dispose();
    }

    #endregion

    #region Methods

    private void OnDataSourcePropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(TableStoreDataSource.ColumnCount))
        {
            UpdateCounts();
        }
    }

    private void OnRowCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        UpdateCounts();
    }

    /// <summary>
    ///     Mirrors the counts shown in the document status bar. The data source raises its change notifications from
    ///     background threads, so the view model properties are only ever touched on the UI thread.
    /// </summary>
    private void UpdateCounts()
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            ApplyCounts();
            return;
        }

        Dispatcher.UIThread.Post(ApplyCounts);
    }

    private void ApplyCounts()
    {
        if (IsDisposed)
        {
            return;
        }

        HeadersCount = _DataSource?.ColumnCount ?? 0;
        RowCount = _DataSource?.Collection.Count ?? 0;
    }

    private void DetachDataSource()
    {
        if (_DataSource is not null)
        {
            _DataSource.PropertyChanged -= OnDataSourcePropertyChanged;
            _DataSource = null;
        }

        if (_RowCollectionChangedSource is not null)
        {
            _RowCollectionChangedSource.CollectionChanged -= OnRowCollectionChanged;
            _RowCollectionChangedSource = null;
        }
    }

    #endregion
}