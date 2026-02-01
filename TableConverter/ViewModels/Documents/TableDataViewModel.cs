using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using ModelFlow.DataVirtualization.DataManagement;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.ViewModels.Base;
using TableConverter.Extensions;
using TableConverter.Services.DataSources;
using TableConverter.Utilities.Database.Events;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Utilities.Extensions;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.ViewModels.Documents;

public partial class TableDataViewModel : BaseDocumentViewModel
{
    #region Properties

    [ObservableProperty] private string _Path;
    [ObservableProperty] private TableStoreDataSource _DataSource;
    [ObservableProperty] private FlatTreeDataGridSource<DataItem<RowEntity>> _TreeDataSource;
    
    public override bool CanClose => !IsDirty;

    private readonly IDatabaseContextFactory<TableStoreDbContext> _dbContextFactory;

    #endregion

    #region Constructors

    public TableDataViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager,
        ISukiToastManager toastManager,
        IDatabaseContextFactory<TableStoreDbContext> dbContextFactory)
        : base(commandManager, eventManager, dialogManager, toastManager)
    {
        _dbContextFactory = dbContextFactory;
        Path = System.IO.Path.Combine(App.AppStorageDirectory, $"{DateTime.Now.ToFileTime()}.tcstore");
        DataSource = new TableStoreDataSource(_dbContextFactory);
        TreeDataSource = new FlatTreeDataGridSource<DataItem<RowEntity>>(DataSource.Collection);
        DataSource.Path = Path;
        
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
        
        _eventManager.GetEvent<DbEntityChangedEvent>()
            .Subscribe(OnEntityChanged);
    }

    #endregion

    #region Methods
    
    public void InvalidateData()
    {
        TreeDataSource.Columns.Clear();
        
        using var dbContext = _dbContextFactory.Create(Path);
        
        dbContext.Columns
            .AsNoTracking()
            .AsEnumerable()
            .ForEach((column, idx) => TreeDataSource.AddAutoColumn(column.Name, idx));
        
        DataSource.Invalidate();
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
        var dataSource = new FlatTreeDataGridSource<DataItem<RowEntity>>(DataSource.Collection);

        await using var dbContext = await _dbContextFactory.CreateAsync(Path);

        var columns = await dbContext.Columns
            .AsNoTracking()
            .OrderBy(x => x.OrdinalPosition)
            .ToListAsync();
            
        columns.ForEach((column, idx) => dataSource.AddAutoColumn(column.Name, idx));
        
        TreeDataSource = dataSource;
    }

    #endregion
}