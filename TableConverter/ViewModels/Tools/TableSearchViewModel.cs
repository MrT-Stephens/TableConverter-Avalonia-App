using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using ModelFlow.DataVirtualization.DataManagement;
using ModelFlow.DataVirtualization.Interfaces;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Handlers.TableData;
using TableConverter.Commands.Interfaces;
using TableConverter.Extensions;
using TableConverter.Interfaces;
using TableConverter.Services.DataSources;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Events;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Utilities.Extensions;
using TableConverter.Utilities.Interfaces;
using TableConverter.ViewModels.Base;
using TableConverter.ViewModels.Documents;
using TableConverter.ViewModels.Forms;
using TableConverter.ViewModels.Workspaces;

namespace TableConverter.ViewModels.Tools;

public partial class TableSearchViewModel : BaseScopedPaneToolViewModel<TableWorkspaceEditorViewModel>
{
    #region Properties
    
    [ObservableProperty] private SearchSettingsFrom _SearchSettings;
    [ObservableProperty] private ObservableCollection<ICommandInstance> _SearchCommands;
    [ObservableProperty] private IReadOnlyObservableCollection<DataItem<SearchResult>> _SearchResults;

    public readonly TableStoreSearchResultDataSource DataSource;
    
    private readonly IDatabaseContextFactory<TableStoreDbContext> _databaseContextFactory;
    
    #endregion
    
    #region Constructors
    
    public TableSearchViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager, 
        ISukiToastManager toastManager,
        IDatabaseContextFactory<TableStoreDbContext> databaseContextFactory)
        : base(commandManager, eventManager, dialogManager, toastManager, "Search & Replace")
    {
        _databaseContextFactory = databaseContextFactory;
        SearchSettings = new SearchSettingsFrom();
        SearchCommands = [];
        DataSource = new TableStoreSearchResultDataSource(databaseContextFactory);
        SearchResults = DataSource.Collection;

        DataSource.SetFilterQuery(query => query
            .OrderBy(x => x.RowId)
            .ThenBy(x => x.ColumnId), false);
        
        Dispatcher.UIThread.Post(async void () =>
        {
            await DataSource.EnsureInitialisedAsync();
        });
    }
    
    #endregion
    
    #region Overrides

    public override void Initialise()
    {
        base.Initialise();
        
        SearchCommands.Add(this[TableDataCommandNames.Search]);
        SearchCommands.Add(this[TableDataCommandNames.Replace]);

        _eventManager.GetEvent<DbEntityChangedEvent>()
            .Subscribe(OnEntityChanged);
    }

    protected override void OnSelectedDocumentChanged(IWorkspace workspace, IPaneDocument? oldDocument, IPaneDocument? newDocument)
    {
        base.OnSelectedDocumentChanged(workspace, oldDocument, newDocument);
        
        if (oldDocument?.ID != newDocument?.ID)
        {
            SearchSettings = new SearchSettingsFrom();

            if (newDocument is TableDataViewModel tableDataViewModel)
            {
                DataSource.Path = tableDataViewModel.Path;
                
                RefreshColumnNames(tableDataViewModel.Path).FireAndForget();
            }
            else
            {
                DataSource.Path = string.Empty;
            }
        }
    }

    #endregion

    #region Private Methods

    private async Task RefreshColumnNames(string path)
    {
        await using var db = await _databaseContextFactory.CreateAsync(path);

        var columnNames = await db.Columns
            .AsNoTracking()
            .OrderBy(c => c.OrdinalPosition)
            .Select(c => c.Name)
            .ToListAsync();
        
        SearchSettings.ColumnNames.Clear();
        SearchSettings.ColumnNames.Add("All");
        SearchSettings.ColumnNames.AddRange(columnNames);
    }

    private void OnEntityChanged(object? sender, DbEntityChangedEventArgs args)
    {
        if (args.Type != typeof(ColumnEntity)
            || string.IsNullOrEmpty(DataSource.Path)
            || DataSource.SourceId != args.SourceId)
        {
            return;
        }

        RefreshDataAsync(args.Changes).FireAndForget();
    }
    
    private async Task RefreshDataAsync(DbEntityChange[] changes)
    {
        foreach (var change in changes)
        {
            if (change.Entity is not ColumnEntity column)
            {
                continue;
            }

            if (change.State is DbEntityChangeState.Deleted)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    SearchSettings.ColumnNames.Remove(column.Name);
                });
            }
            else if (change.State is DbEntityChangeState.Added)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    SearchSettings.ColumnNames.Insert(column.OrdinalPosition - 1, column.Name);
                });
            }
            else if (change.State is DbEntityChangeState.Modified
                && change.ModifiedProperties.TryGetValue(nameof(ColumnEntity.Name), out var values)
                && values is { Original: string originalName, Current: string currentName })
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    SearchSettings.ColumnNames.Replace(originalName, currentName);
                });
            }
        }
    }

    #endregion
}