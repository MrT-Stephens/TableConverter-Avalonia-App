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
    
    private readonly ITableStoreDbContextFactory _databaseContextFactory;
    
    #endregion
    
    #region Constructors
    
    public TableSearchViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager, 
        ISukiToastManager toastManager,
        ITableStoreDbContextFactory databaseContextFactory)
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
        
        // Start the data source initialisation on the UI thread without the async void anti-pattern.
        Dispatcher.UIThread.Post(() => DataSource.EnsureInitialisedAsync().FireAndForget());
    }
    
    #endregion
    
    #region Overrides

    public override void Initialise()
    {
        base.Initialise();
        
        SearchCommands.Add(this[TableDataCommandNames.Search]);
        SearchCommands.Add(this[TableDataCommandNames.Replace]);

        _eventRegistrar.RegisterSubscription(
            _eventManager.GetEvent<DbEntityChangedEvent>(),
            OnEntityChanged);
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
                
                RefreshColumnNamesAsync().FireAndForget();
            }
            else
            {
                DataSource.Path = string.Empty;
            }
        }
    }

    #endregion

    #region Column Names

    /// <summary>
    ///     Reads the column names of the document's table back into the search settings.
    /// </summary>
    public Task RefreshColumnNamesAsync()
    {
        return string.IsNullOrEmpty(DataSource.Path)
            ? Task.CompletedTask
            : RefreshColumnNames(DataSource.Path);
    }

    private async Task RefreshColumnNames(string path)
    {
        await using var db = await _databaseContextFactory.CreateDbContextAsync(path);

        var columnNames = await db.Columns
            .AsNoTracking()
            .OrderBy(c => c.OrdinalPosition)
            .Select(c => c.Name)
            .ToListAsync();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            SearchSettings.ColumnNames.Clear();
            SearchSettings.ColumnNames.Add("All");
            SearchSettings.ColumnNames.AddRange(columnNames);

            // A column that has been renamed or removed no longer names anything to search in, so the
            // selection falls back to every column rather than pointing at a name that is not there.
            if (!SearchSettings.ColumnNames.Contains(SearchSettings.SearchInSpecificColumn))
            {
                SearchSettings.SearchInSpecificColumn = "All";
            }
        });
    }

    private void OnEntityChanged(object? sender, DbEntityChangedEventArgs args)
    {
        if (args.Type != typeof(ColumnEntity)
            || string.IsNullOrEmpty(DataSource.Path)
            || DataSource.SourceId != args.SourceId)
        {
            return;
        }

        // The names are read back from the store rather than patched one change at a time: a column can be
        // renamed by a statement that never reaches the change tracker - replacing a header does - so
        // patching would leave the list holding names the store no longer has, and the next change, such as
        // undoing the replace, would not find the name it expects to replace.
        RefreshColumnNamesAsync().FireAndForget();
    }
    
    #endregion
}