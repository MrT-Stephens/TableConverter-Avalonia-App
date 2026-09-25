using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ModelFlow.DataVirtualization.DataManagement;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Configuration;
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

    private readonly ITableStoreDbContextFactory _dbContextFactory;
    private readonly IOptions<AppOptions> _appOptions;

    #endregion

    #region Constructors

    public TableDataViewModel(
        ICommandManager commandManager, 
        IEventManager eventManager, 
        ISukiDialogManager dialogManager,
        ISukiToastManager toastManager,
        ITableStoreDbContextFactory dbContextFactory,
        IOptions<AppOptions> appOptions)
        : base(commandManager, eventManager, dialogManager, toastManager)
    {
        _appOptions = appOptions;
        _dbContextFactory = dbContextFactory;
        Path = string.Empty;
        DataSource = new TableStoreDataSource(_dbContextFactory);
        TreeDataSource = new FlatTreeDataGridSource<DataItem<RowEntity>>(DataSource.Collection);
        TreeDataSource.RowSelection!.SingleSelect = false; 
        DataSource.Path = Path;
        
        // Start the data source initialisation on the UI thread without the async void anti-pattern.
        Dispatcher.UIThread.Post(() => DataSource.EnsureInitialisedAsync().FireAndForget());
    }

    #endregion

    #region Overrides

    public override void Initialise()
    {
        base.Initialise();

        var path = System.IO.Path.Combine(
            _appOptions.Value.BaseDocumentsPath, 
            $"{DateTime.UtcNow.ToFileTime()}.tcstore");
        
        Path = path;
        DataSource.Path = path;

        _eventRegistrar.RegisterSubscription(
            _eventManager.GetEvent<DbEntityChangedEvent>(),
            OnEntityChanged);

        _eventRegistrar.RegisterEvent<EventHandler<TreeSelectionModelSelectionChangedEventArgs<DataItem<RowEntity>>>>(
            action => TreeDataSource.RowSelection!.SelectionChanged += action,
            action => TreeDataSource.RowSelection!.SelectionChanged -= action, 
            null, (_, args) =>
            {
                args.DeselectedItems.ForEach(item => SelectedItems.Remove(item));
                args.SelectedItems.ForEach(item => SelectedItems.Add(item));
            });
    }

    #endregion

    #region Methods
    
    public async Task InvalidateDataAsync()
    {
        if (string.IsNullOrEmpty(Path))
        {
            return;
        }

        // Read the columns off the UI thread: querying the store is not rendering work, so it must not
        // block the UI.
        List<ColumnEntity> columns;

        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync(Path).ConfigureAwait(false))
        {
            columns = await dbContext.Columns
                .AsNoTracking()
                .OrderBy(c => c.OrdinalPosition)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        // The grid columns are UI state, so they are rebuilt on the UI thread.
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            TreeDataSource.Columns.Clear();

            foreach (var column in columns)
            {
                var newColumn = CreateTemplateColumn<DataItem<RowEntity>>(column.Name, column.OrdinalPosition - 1);
                TreeDataSource.Columns.Insert(column.OrdinalPosition - 1, newColumn);
            }

            DataSource.Invalidate();
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
                    TreeDataSource.Columns.RemoveAt(column.OrdinalPosition - 1);
                });
            }
            else if (change.State is DbEntityChangeState.Added)
            {
                var newColumn = CreateTemplateColumn<DataItem<RowEntity>>(column.Name, column.OrdinalPosition - 1);
                
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    TreeDataSource.Columns.Insert(column.OrdinalPosition - 1, newColumn);
                });
            }
            else if (change.State is DbEntityChangeState.Modified)
            {
                var newColumn = CreateTemplateColumn<DataItem<RowEntity>>(column.Name, column.OrdinalPosition - 1);
                
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    TreeDataSource.Columns.RemoveAt(column.OrdinalPosition - 1);
                    TreeDataSource.Columns.Insert(column.OrdinalPosition - 1, newColumn);
                });
            }
        }
    }

    private static TemplateColumn<TModel> CreateTemplateColumn<TModel>(object header, int columnIndex, GridLength? gridLength = null) 
        where TModel : class
    {
        return new TemplateColumn<TModel>(header,
            new FuncDataTemplate<TModel>((_, _) => new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                [!TextBlock.TextProperty] = new Binding
                {
                    Path = $"Item.Cells[{columnIndex}].Value",
                    Mode = BindingMode.TwoWay
                },
            }),
            new FuncDataTemplate<TModel>((_, _) => new TextBox
            {
                VerticalAlignment = VerticalAlignment.Center,
                [!TextBox.TextProperty] = new Binding
                {
                    Path = $"Item.Cells[{columnIndex}].Value",
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.LostFocus
                }
            }),
            GridLength.Auto,
            new TemplateColumnOptions<TModel>
            {
                CanUserSortColumn = false,
            });
    }

    #endregion
}