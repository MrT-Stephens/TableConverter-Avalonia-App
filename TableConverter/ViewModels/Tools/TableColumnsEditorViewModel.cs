using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Selection;
using Avalonia.Data;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using ModelFlow.DataVirtualization.DataManagement;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Handlers.TableData;
using TableConverter.Commands.Interfaces;
using TableConverter.Extensions;
using TableConverter.Interfaces;
using TableConverter.Services.DataSources;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.Events;
using TableConverter.Utilities.Database.History;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Utilities.Extensions;
using TableConverter.Utilities.Interfaces;
using TableConverter.Utilities.Models;
using TableConverter.ViewModels.Base;
using TableConverter.ViewModels.Documents;
using TableConverter.ViewModels.Workspaces;

namespace TableConverter.ViewModels.Tools;

public partial class TableColumnsEditorViewModel : BaseScopedPaneToolViewModel<TableWorkspaceEditorViewModel>
{
    #region Properties
    
    [ObservableProperty] private ObservableCollection<ICommandInstance> _ColumnCommands;
    [ObservableProperty] private FlatTreeDataGridSource<DataItem<ColumnEntity>> _TreeDataSource;

    /// <summary>
    ///     Whether the column under the cursor has been summarised. False while nothing, or more than one
    ///     thing, is selected, so the readout can be hidden rather than shown empty.
    /// </summary>
    [ObservableProperty] private bool _HasColumnStatistics;

    /// <summary>
    ///     How full the column under the cursor is, in a form short enough to sit under the grid.
    /// </summary>
    [ObservableProperty] private string _StatisticsSummary;

    public readonly TableStoreColumnsDataSource DataSource;
    
    private readonly ITableStoreDbContextFactory _dbContextFactory;

    /// <summary>
    ///     How many times the counts have been asked for. A read overtaken by a later one is dropped rather
    ///     than shown against whichever column is selected now.
    /// </summary>
    private int _statisticsGeneration;

    /// <summary>
    ///     The column under the cursor, when exactly one thing is selected.
    /// </summary>
    private ColumnEntity? SelectedColumn =>
        TryGetSelectedItem<DataItem<ColumnEntity>>(out var item) ? item.Item : null;

    #endregion
    
    #region Constructor
    
    public TableColumnsEditorViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager, 
        ISukiToastManager toastManager,
        ITableStoreDbContextFactory databaseContextFactory,
        ITableHistory history) 
        : base(commandManager, eventManager, dialogManager, toastManager, "Columns Editor", false)
    {
        _dbContextFactory = databaseContextFactory;
        _statisticsGeneration = 0;
        StatisticsSummary = string.Empty;
        ColumnCommands = [];
        DataSource = new TableStoreColumnsDataSource(databaseContextFactory, history);
        TreeDataSource = new FlatTreeDataGridSource<DataItem<ColumnEntity>>(DataSource.Collection);
        TreeDataSource.RowSelection!.SingleSelect = false; 
        
        TreeDataSource
            .AddAutoColumn("ID", "Item.OrdinalPosition", true)
            .AddAutoColumn("Name", "Item.Name", sourceTrigger: UpdateSourceTrigger.LostFocus, gridLength: GridLength.Star)
            .AddEnumColumn<DataItem<ColumnEntity>, ColumnDataType>("Type", "Item.DataType")
            .AddAutoColumn("Default Value", "Item.DefaultValueForCell", sourceTrigger: UpdateSourceTrigger.LostFocus);

        DataSource.SetFilterQuery(query => query
            .OrderBy(x => x.OrdinalPosition));
        
        // Start the data source initialisation on the UI thread without the async void anti-pattern.
        Dispatcher.UIThread.Post(() => DataSource.EnsureInitialisedAsync().FireAndForget());
    }
    
    #endregion

    #region Overrides

    public override void Initialise()
    {
        base.Initialise();
        
        ColumnCommands.Add(this[TableDataCommandNames.AddColumn]);
        ColumnCommands.Add(this[TableDataCommandNames.DuplicateColumn]);
        ColumnCommands.Add(this[TableDataCommandNames.MoveColumnLeft]);
        ColumnCommands.Add(this[TableDataCommandNames.MoveColumnRight]);
        ColumnCommands.Add(this[TableDataCommandNames.DeleteColumn]);
        
        _eventRegistrar.RegisterEvent<EventHandler<TreeSelectionModelSelectionChangedEventArgs<DataItem<ColumnEntity>>>>(
            action => TreeDataSource.RowSelection!.SelectionChanged += action,
            action => TreeDataSource.RowSelection!.SelectionChanged -= action, 
            null, (_, args) =>
            {
                args.DeselectedItems.ForEach(item => SelectedItems.Remove(item));
                args.SelectedItems.ForEach(item => SelectedItems.Add(item));

                UpdateStatisticsAsync().FireAndForget();
            });

        // An edit the store refuses is not written, but the row the grid is showing still holds what was
        // typed: the notice says what was wrong and the columns are read again so the row reads the way the
        // store holds it.
        _eventRegistrar.RegisterEvent<EventHandler<ColumnEditRejectedEventArgs>>(
            action => DataSource.EditRejected += action,
            action => DataSource.EditRejected -= action,
            null, (_, args) => OnEditRejected(args));
    }

    public override void OnActivate()
    {
        base.OnActivate();

        // Cells are edited straight in the table's own grid, which this tool cannot see. Reading the counts
        // again when the tool is brought back to the front keeps the readout in step with the table.
        UpdateStatisticsAsync().FireAndForget();
    }

    protected override void OnSelectedDocumentChanged(IWorkspace workspace, IPaneDocument? oldDocument, IPaneDocument? newDocument)
    {
        base.OnSelectedDocumentChanged(workspace, oldDocument, newDocument);
        
        if (oldDocument?.ID != newDocument?.ID)
        {
            if (newDocument is TableDataViewModel tableDataViewModel)
            {
                DataSource.Path = tableDataViewModel.Path;
            }
            else
            {
                DataSource.Path = string.Empty;
            }
            
            SelectedItems.RemoveAll<DataItem<ColumnEntity>>();

            UpdateStatisticsAsync().FireAndForget();
        }
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Puts the selection back on a column the tool has just added or moved, so the same column stays
    ///     under the cursor and can be worked on again without hunting for it.
    /// </summary>
    /// <param name="columnId">The column to select.</param>
    /// <remarks>
    ///     Reading the columns again replaces every item the grid was showing, so the item the selection was
    ///     holding is no longer the one that shows the column. The column is found again by its id, once the
    ///     page it sits on has been read.
    /// </remarks>
    public async Task SelectColumnAsync(int columnId)
    {
        // The items the selection was holding belong to the read that was just thrown away.
        SelectedItems.RemoveAll<DataItem<ColumnEntity>>();

        await DataSource.EnsureInitialisedAsync().ConfigureAwait(false);

        for (var attempt = 0; attempt < 50; attempt++)
        {
            var index = FindColumnIndex(columnId);

            if (index >= 0)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    TreeDataSource.RowSelection!.SelectedIndex = index;
                });

                return;
            }

            await Task.Delay(20).ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     Finds the row the grid is showing a column in.
    /// </summary>
    /// <param name="columnId">The column to find.</param>
    /// <returns>The row the column is shown in, or -1 while it has not been read yet.</returns>
    private int FindColumnIndex(int columnId)
    {
        var collection = DataSource.Collection;

        for (var index = 0; index < collection.Count; index++)
        {
            // A row the data source has not read yet is a placeholder, and its item is not a column of the
            // table at all.
            if (collection[index] is { IsLoading: false } item && item.Item.Id == columnId)
            {
                return index;
            }
        }

        return -1;
    }

    /// <summary>
    ///     Reads back what the column under the cursor holds, so the tool can say how full the column is
    ///     rather than only what it is called.
    /// </summary>
    private async Task UpdateStatisticsAsync()
    {
        var generation = ++_statisticsGeneration;

        var column = SelectedColumn;
        var path = DataSource.Path;

        if (column is null || string.IsNullOrEmpty(path))
        {
            ApplyStatistics(null, null);
            return;
        }

        TableStatistics statistics;

        // Counting the values is store work rather than drawing work, so it is read away from the UI thread
        // and only the result is handed back to it.
        await using (var db = await _dbContextFactory.CreateDbContextAsync(path).ConfigureAwait(false))
        {
            statistics = await TableStoreStatistics.ReadAsync(db).ConfigureAwait(false);
        }

        // A selection can move on while the counts are being read, so a read overtaken by a later one is
        // dropped rather than shown against whichever column is selected now.
        if (generation != _statisticsGeneration)
        {
            return;
        }

        var perColumn = statistics.Columns
            .FirstOrDefault(candidate => candidate.OrdinalPosition == column.OrdinalPosition);

        ApplyStatistics(statistics, perColumn);
    }

    private void ApplyStatistics(TableStatistics? table, ColumnStatistics? column)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            ApplyStatisticsCore(table, column);
            return;
        }

        Dispatcher.UIThread.Post(() => ApplyStatisticsCore(table, column));
    }

    private void ApplyStatisticsCore(TableStatistics? table, ColumnStatistics? column)
    {
        if (IsDisposed)
        {
            return;
        }

        HasColumnStatistics = table is not null && column is not null;

        StatisticsSummary = HasColumnStatistics
            ? $"{table!.RowCount:N0} rows · {column!.EmptyCount:N0} blank · {column.DistinctCount:N0} distinct"
            : string.Empty;
    }

    private void OnEditRejected(ColumnEditRejectedEventArgs args)
    {
        // The store raises this from whichever thread refused the edit, and the notice and the read that
        // follows it belong to the UI.
        if (Dispatcher.UIThread.CheckAccess())
        {
            ShowRejection(args);
            return;
        }

        Dispatcher.UIThread.Post(() => ShowRejection(args));
    }

    private void ShowRejection(ColumnEditRejectedEventArgs args)
    {
        if (IsDisposed)
        {
            return;
        }

        _toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Error)
            .WithTitle("Column Edit Refused")
            .WithContent(args.Reason)
            .Queue();

        // The grid is still showing what the store refused, so the columns are read again to put the row
        // back the way the store holds it.
        DataSource.Invalidate();
    }

    #endregion
}