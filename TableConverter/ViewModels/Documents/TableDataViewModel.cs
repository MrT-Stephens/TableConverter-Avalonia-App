using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ModelFlow.DataVirtualization.DataManagement;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Common;
using TableConverter.Interfaces;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.ViewModels.Base;
using Path = System.IO.Path;
using TableConverter.Extensions;
using TableConverter.Services.DataSources;
using TableConverter.Utilities.Database.Models;
using TableConverter.Utilities.Extensions;

namespace TableConverter.ViewModels.Documents;

public partial class TableDataViewModel : BaseDocumentViewModel
{
    #region Properties

    public override bool CanClose => !IsDirty;
    public override bool CanUndo { get; }
    public override bool CanRedo { get; }
    
    public IDataGridCollectionView CollectionView { get; }
    public TableStoreDataSource DataSource { get; }
    public TableStoreDbContext DbContext { get; }

    [ObservableProperty] private ObservableCollection<string> _Headers;

    #endregion

    #region  Constructors

    public TableDataViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager,
        ISukiToastManager toastManager,
        IUndoRedo undoRedo,
        IDbContextFactory<TableStoreDbContext> dbContextFactory)
        : base(commandManager, eventManager, dialogManager, toastManager, undoRedo)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TableConverter", "TestData.csv");
        
        DbContext = dbContextFactory.Create(path);
        DataSource = new TableStoreDataSource(dbContextFactory, path);

        Headers = DbContext.Columns
            .Select(x => x.Name)
            .AsEnumerable()
            .ToObservableCollection();

        CollectionView = new VirtualisingCollectionView(DataSource.Collection);
        
        Dispatcher.UIThread.Post(async void () =>
        {
            await DataSource.EnsureInitialisedAsync();
        });
    }

    #endregion

    #region Methods

    #endregion
}