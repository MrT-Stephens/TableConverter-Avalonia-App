using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ModelFlow.DataVirtualization.DataManagement;
using ModelFlow.DataVirtualization.Extensions;
using ModelFlow.DataVirtualization.Interfaces;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Handlers.TableData;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;
using TableConverter.Services.DataSources;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Database.Models.TableStore;
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
    
    #endregion
    
    #region Constructors
    
    public TableSearchViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager, 
        ISukiToastManager toastManager,
        IDbContextFactory<TableStoreDbContext> dbContextFactory)
        : base(commandManager, eventManager, dialogManager, toastManager, "Search & Replace")
    {
        SearchSettings = new SearchSettingsFrom();
        SearchCommands = [];
        
        DataSource = new TableStoreSearchResultDataSource(dbContextFactory);
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
    }

    protected override void OnSelectedDocumentChanged(IWorkspace workspace, IPaneDocument? oldDocument, IPaneDocument? newDocument)
    {
        base.OnSelectedDocumentChanged(workspace, oldDocument, newDocument);
        
        if (oldDocument?.ID != newDocument?.ID)
        {
            SearchSettings = new SearchSettingsFrom();
            
            DataSource.Path = newDocument is TableDataViewModel tableData
                ? tableData.Path
                : string.Empty;
        }
    }

    #endregion
}