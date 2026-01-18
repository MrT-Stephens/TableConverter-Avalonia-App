using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ModelFlow.DataVirtualization.DataManagement;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Handlers.TableData;
using TableConverter.Commands.Interfaces;
using TableConverter.Extensions;
using TableConverter.Interfaces;
using TableConverter.Services.DataSources;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Utilities.Extensions;
using TableConverter.Utilities.Interfaces;
using TableConverter.ViewModels.Base;
using TableConverter.ViewModels.Documents;
using TableConverter.ViewModels.Workspaces;

namespace TableConverter.ViewModels.Tools;

public partial class TableColumnsEditorViewModel : BaseScopedPaneToolViewModel<TableWorkspaceEditorViewModel>
{
    #region Properties
    
    [ObservableProperty] private ObservableCollection<ICommandInstance> _ColumnCommands;
    [ObservableProperty] private FlatTreeDataGridSource<DataItem<ColumnEntity>> _TreeDataSource;

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
        ColumnCommands = [];
        DataSource = new TableStoreColumnsDataSource(dbContextFactory);
        TreeDataSource = new FlatTreeDataGridSource<DataItem<ColumnEntity>>(DataSource.Collection);
        TreeDataSource.RowSelection!.SingleSelect = false; 
        
        _eventRegistrar.RegisterEvent<EventHandler<TreeSelectionModelSelectionChangedEventArgs<DataItem<ColumnEntity>>>>(
            action => TreeDataSource.RowSelection!.SelectionChanged += action,
            action => TreeDataSource.RowSelection!.SelectionChanged -= action, 
            null, (_, args) =>
            {
                args.DeselectedItems.ForEach(item => SelectedItems.Remove(item));
                args.SelectedItems.ForEach(item => SelectedItems.Add(item));
            });
        
        TreeDataSource
            .AddAutoColumn("ID", "Item.Id", true)
            .AddAutoColumn("Name", "Item.Name")
            .AddAutoColumn("Data Type", "Item.DataType")
            .AddAutoColumn("Default Value", "Item.DefaultValueForCell");

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
        ColumnCommands.Add(this[TableDataCommandNames.DeleteColumn]);
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
        }
    }

    #endregion
}