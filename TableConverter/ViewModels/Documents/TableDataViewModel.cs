using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ModelFlow.DataVirtualization.DataManagement;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.ViewModels.Base;
using TableConverter.Extensions;
using TableConverter.Services.DataSources;
using TableConverter.Utilities.Database.Models;
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

    private readonly IDbContextFactory<TableStoreDbContext> _dbContextFactory;

    #endregion

    #region Constructors

    public TableDataViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager,
        ISukiToastManager toastManager,
        IDbContextFactory<TableStoreDbContext> dbContextFactory)
        : base(commandManager, eventManager, dialogManager, toastManager)
    {
        _dbContextFactory = dbContextFactory;
        _Path = null!;
        _DataSource = null!;
        _TreeDataSource = null!;
    }

    #endregion

    #region Overrides

    public override void Initialise()
    {
        base.Initialise();
        
        var path = System.IO.Path.Combine(App.AppStorageDirectory, $"{ID}.tcstore");

        Path = path;
        DataSource = new TableStoreDataSource(_dbContextFactory, path);
        TreeDataSource = new FlatTreeDataGridSource<DataItem<RowEntity>>(DataSource.Collection);

        var dbContext = _dbContextFactory.Create(path);
        
        dbContext.Columns
            .AsEnumerable()
            .ForEach(column => TreeDataSource.AddAutoColumn(column.Name, column.Ordinal));
        
        Dispatcher.UIThread.Post(async void () =>
        {
            await DataSource.EnsureInitialisedAsync();
        });
    }

    #endregion

    #region Methods
    
    public void InvalidateData()
    {
        DataSource.Invalidate();
        TreeDataSource.Columns.Clear();
        
        using var dbContext = _dbContextFactory.Create(Path);
        
        dbContext.Columns
            .AsEnumerable()
            .ForEach((column, idx) => TreeDataSource.AddAutoColumn(column.Name, idx));
    }

    #endregion
}