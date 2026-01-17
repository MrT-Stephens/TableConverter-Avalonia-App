using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ModelFlow.DataVirtualization.DataManagement;
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
using TableConverter.ViewModels.Workspaces;

namespace TableConverter.ViewModels.Tools;

public partial class TableColumnsEditorViewModel : BaseScopedPaneToolViewModel<TableWorkspaceEditorViewModel>
{
    #region Properties

    [ObservableProperty] private ObservableCollection<DataItem<ColumnEntity>> _SelectedColumns;
    [ObservableProperty] private ObservableCollection<ICommandInstance> _ColumnCommands;
    [ObservableProperty] private IReadOnlyObservableCollection<DataItem<ColumnEntity>> _Columns;

    public readonly TableStoreColumnsDataSource DataSource;

    #endregion
    
    #region Constructor
    
    public TableColumnsEditorViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager, 
        ISukiToastManager toastManager,
        IDbContextFactory<TableStoreDbContext> dbContextFactory) 
        : base(commandManager, eventManager, dialogManager, toastManager, "Columns Editor")
    {
        SelectedColumns = [];
        ColumnCommands = [];
        DataSource = new TableStoreColumnsDataSource(dbContextFactory);
        Columns = DataSource.Collection;

        DataSource.SetFilterQuery(query => query
            .OrderBy(x => x.Id));
        
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
        
        ColumnCommands.Add(this[TableDataCommandNames.EditColumn]);
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
        }
    }

    #endregion
}