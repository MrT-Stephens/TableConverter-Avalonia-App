using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ModelFlow.DataVirtualization.DataManagement;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Configuration;
using TableConverter.Contracts;
using TableConverter.Converters;
using TableConverter.Interfaces;
using TableConverter.ViewModels.Base;
using TableConverter.Extensions;
using TableConverter.Services.DataSources;
using TableConverter.Utilities;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.Events;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Utilities.Extensions;
using TableConverter.Utilities.Interfaces;

namespace TableConverter.ViewModels.Documents;

public partial class TableDataViewModel : BaseDocumentViewModel, ISessionDocument
{
    #region Properties

    /// <summary>
    /// The key table data documents are filed under in the session. Entries are grouped by it, so a
    /// document type that stores its data differently never disturbs these documents.
    /// </summary>
    public const string SessionDocumentType = "table-data";

    [ObservableProperty] private string _Path;
    [ObservableProperty] private TableStoreDataSource _DataSource;
    [ObservableProperty] private FlatTreeDataGridSource<DataItem<RowEntity>> _TreeDataSource;

    /// <summary>
    /// <see langword="true" /> while the store is scratch data the application created itself, which
    /// is what makes it safe to delete when the document is closed. Stores the user opened from disk
    /// are theirs and are never removed by the application.
    /// </summary>
    [ObservableProperty] private bool _IsTemporaryStore = true;

    /// <summary>
    /// Cells are edited straight through to the store, so a table document never holds unsaved state
    /// and can always be closed.
    /// </summary>
    public override bool CanClose => true;

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
        
        // The store is assigned later, through LoadAsync, so nothing touches the file system until
        // the document is actually given a table store to show.
    }

    #endregion

    #region Overrides

    public override void Initialise()
    {
        base.Initialise();

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
                var newColumn = CreateTemplateColumn<DataItem<RowEntity>>(
                    column.Name, column.OrdinalPosition - 1, column.DataType);

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
                var newColumn = CreateTemplateColumn<DataItem<RowEntity>>(
                    column.Name, column.OrdinalPosition - 1, column.DataType);
                
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    TreeDataSource.Columns.Insert(column.OrdinalPosition - 1, newColumn);
                });
            }
            else if (change.State is DbEntityChangeState.Modified)
            {
                // A column that was retyped is rebuilt rather than patched: the type is what decides how
                // its cells are drawn, so the whole column has to be replaced.
                var newColumn = CreateTemplateColumn<DataItem<RowEntity>>(
                    column.Name, column.OrdinalPosition - 1, column.DataType);
                
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    TreeDataSource.Columns.RemoveAt(column.OrdinalPosition - 1);
                    TreeDataSource.Columns.Insert(column.OrdinalPosition - 1, newColumn);
                });
            }
        }
    }

    /// <summary>
    /// Builds the grid column that shows the values of one table column.
    /// </summary>
    /// <param name="header">The column heading, which is the column's name.</param>
    /// <param name="columnIndex">The position of the column within a row's cells.</param>
    /// <param name="dataType">The type the column was given, which decides how its cells read.</param>
    /// <param name="gridLength">The width to give the column.</param>
    private static TemplateColumn<TModel> CreateTemplateColumn<TModel>(
        object header,
        int columnIndex,
        ColumnDataType dataType,
        GridLength? gridLength = null) 
        where TModel : class
    {
        var valuePath = $"Item.Cells[{columnIndex}].Value";

        // A value is never rejected for not matching its column's type, because a typed column would
        // otherwise be unusable while its values were still being entered. The type only decides how the
        // cell reads: numbers are right aligned, so a column of them lines up the way it would in a
        // spreadsheet, and anything that does not read as the type is marked, so the styles can draw it
        // in the theme's error colour.
        var textAlignment = dataType.IsNumeric() ? TextAlignment.Right : TextAlignment.Left;
        var mismatch = new ColumnValueMismatchConverter(dataType);

        return new TemplateColumn<TModel>(header,
            new FuncDataTemplate<TModel>((_, _) => new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = textAlignment,
                [!TextBlock.TextProperty] = new Binding
                {
                    Path = valuePath,
                    Mode = BindingMode.TwoWay
                },
                [!ColumnValueMismatch.IsMismatchedProperty] = new Binding
                {
                    Path = valuePath,
                    Converter = mismatch
                }
            }),
            new FuncDataTemplate<TModel>((_, _) => new TextBox
            {
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = textAlignment,
                [!TextBox.TextProperty] = new Binding
                {
                    Path = valuePath,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.LostFocus
                },
                [!ColumnValueMismatch.IsMismatchedProperty] = new Binding
                {
                    Path = valuePath,
                    Converter = mismatch
                }
            }),
            gridLength ?? GridLength.Auto,
            new TemplateColumnOptions<TModel>
            {
                CanUserSortColumn = false,
            });
    }

    #endregion

    #region Loading

    /// <summary>
    /// Reserves the path of a new store inside the documents directory. The file itself is created on
    /// first use, so reserving a path that is never loaded leaves no garbage behind.
    /// </summary>
    public string ReserveStorePath()
    {
        var directory = _appOptions.Value.BaseDocumentsPath;

        System.IO.Directory.CreateDirectory(directory);

        // A random name rather than a timestamp: two documents created within the same tick used to
        // collide on the same file.
        return System.IO.Path.Combine(directory, $"{Guid.NewGuid():N}{TableStoreFile.Extension}");

    }

    /// <summary>
    /// Creates a new, application owned store in the documents directory and loads it.
    /// </summary>
    public Task CreateNewStoreAsync(string? title = null)
    {
        return LoadAsync(ReserveStorePath(), title, true);
    }

    /// <summary>
    /// Opens an existing store from disk.
    /// </summary>
    /// <param name="path">The file to open.</param>
    /// <param name="title">The title to show for the document. Defaults to the file name.</param>
    public Task OpenStoreAsync(string path, string? title = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return LoadAsync(path, title, false);
    }

    /// <summary>
    /// Points the document at <paramref name="path" /> and loads its columns and rows.
    /// </summary>
    /// <param name="path">The table store to load.</param>
    /// <param name="title">The title to show. The current title is kept when empty.</param>
    /// <param name="isTemporaryStore">Whether the store is owned by the application.</param>
    public async Task LoadAsync(string path, string? title = null, bool isTemporaryStore = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        IsTemporaryStore = isTemporaryStore;
        Path = path;

        if (!string.IsNullOrEmpty(title))
        {
            Title = title;
        }

        // Setting the path on the data source also invalidates it, so the rows are re-read from the
        // new store rather than the previous one.
        DataSource.Path = path;

        await InvalidateDataAsync();

        // The grid virtualises over the data source, so it has to be primed before the first render.
        await DataSource.EnsureInitialisedAsync();
    }

    #endregion

    #region Import

    /// <summary>
    /// Replaces everything in this document's store with <paramref name="tableData"/>. Used to fill a
    /// newly created store with the result of an import.
    /// </summary>
    public async Task ImportDataAsync(TableData tableData)
    {
        ArgumentNullException.ThrowIfNull(tableData);

        if (string.IsNullOrEmpty(Path))
        {
            throw new InvalidOperationException(
                "The document has no table store. Call CreateNewStoreAsync before importing data.");
        }

        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync(Path).ConfigureAwait(false))
        {
            await TableStoreDataWriter.WriteAsync(dbContext, tableData).ConfigureAwait(false);
        }

        await InvalidateDataAsync();
    }

    #endregion

    #region Export

    /// <summary>
    /// Reads the whole store into a <see cref="TableData" />, ready to be written out by one of the
    /// file converters.
    /// </summary>
    /// <exception cref="InvalidOperationException">The document has no table store.</exception>
    public async Task<TableData> ReadTableDataAsync()
    {
        if (string.IsNullOrEmpty(Path))
        {
            throw new InvalidOperationException(
                "The document has no table store, so there is nothing to export.");
        }

        // Reading the store is not rendering work, so it stays off the UI thread.
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(Path).ConfigureAwait(false);

        return await TableStoreDataReader.ReadAsync(dbContext).ConfigureAwait(false);
    }

    #endregion

    #region Session

    string ISessionDocument.DocumentType => SessionDocumentType;

    /// <summary>
    /// Captures the store this document shows plus how it was opened, which is all it takes to reopen
    /// it. The store itself is written as the user edits, so nothing of the table has to be captured.
    /// </summary>
    JsonElement ISessionDocument.CaptureSessionState()
    {
        return JsonSerializer.SerializeToElement(new TableDataDocumentState(Path, Title, IsTemporaryStore));
    }

    async Task ISessionDocument.RestoreSessionStateAsync(JsonElement state)
    {
        var restored = state.Deserialize<TableDataDocumentState>()
            ?? throw new InvalidOperationException("The stored table data document state could not be read.");

        // A store that has gone missing must not be reopened: SQLite creates the file on first use, so
        // opening a path that is no longer there would replace a lost document with an empty one.
        if (!System.IO.File.Exists(restored.Path))
        {
            throw new System.IO.FileNotFoundException(
                $"The table store '{restored.Path}' is no longer on disk.", restored.Path);
        }

        await LoadAsync(restored.Path, restored.Title, restored.IsTemporaryStore);
    }

    #endregion
}