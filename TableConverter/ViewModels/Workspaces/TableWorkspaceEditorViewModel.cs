using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Handlers.TableData;
using TableConverter.Extensions;
using TableConverter.Interfaces;
using TableConverter.Utilities.Extensions;
using TableConverter.ViewModels.Base;
using TableConverter.ViewModels.Documents;

namespace TableConverter.ViewModels.Workspaces;

public partial class TableWorkspaceEditorViewModel : BaseWorkspaceEditorViewModel
{
    #region Fields

    private readonly ITableStoreFiles _tableStoreFiles;

    #endregion

    #region Constructors
    
    public TableWorkspaceEditorViewModel(IServiceProvider serviceProvider) 
        : base(serviceProvider, "Table Editor", "TableIcon", 2)
    {
        _tableStoreFiles = serviceProvider.GetRequiredService<ITableStoreFiles>();
    }
    
    #endregion

    #region Overrides

    /// <summary>
    /// Table data documents keep their table in a store file, so they are the only document type this
    /// workspace restores from the session. Reading and writing the session is the base class's job;
    /// how a table itself is stored is described by <see cref="TableDataViewModel" /> and
    /// <see cref="ITableStoreFiles" />.
    /// </summary>
    protected override IReadOnlyCollection<string> DocumentTypes => [TableDataViewModel.SessionDocumentType];

    public override void Initialise()
    {
        base.Initialise();

        // The main menu is composed here rather than described by each command, so what each group holds,
        // what order the groups run in and where the commands sit within them are all decided in one
        // place. Groups and commands are shown in the order they are added, and a command left out is
        // simply not offered by the menu.
        MainMenu = new CommandMenu()
            .Group("File", group => group
                .Add(this[TableDataCommandNames.NewFile], this[TableDataCommandNames.OpenFile])
                .Section()
                .Add(this[TableDataCommandNames.ImportFile])
                .Section()
                .Add(this[TableDataCommandNames.ExportFile]))
            .Group("Edit", group => group
                // Undo and redo walk the table's own history rather than the grid's, so they are offered
                // by the workspace menu and bound to the familiar keys.
                .Add(this[TableDataCommandNames.Undo], this[TableDataCommandNames.Redo])
                .Section()
                .Add(this[TableDataCommandNames.AddRow], this[TableDataCommandNames.DeleteRows]))
            .Group("View", group => group
                .Add(this[TableDataCommandNames.Search]))
            .Group("Tools", group => group
                // The table tools are commands too, so the same actions are offered by the workspace menu
                // and by the table utilities tool, with one implementation behind both.
                .Add(this[TableDataCommandNames.TrimWhitespace], this[TableDataCommandNames.RemoveDuplicateRows])
                .Section()
                .Add(this[TableDataCommandNames.SortByColumn], this[TableDataCommandNames.TransposeClockwise])
                .Section()
                .Add(this[TableDataCommandNames.TransposeCounterClockwise]));
    }

    public override IPaneDocument CreateNewDocumentInstance()
    {
        var document = _serviceProvider.GetRequiredService<TableDataViewModel>();
        document.Initialise();
        return document;
    }

    protected override Task<IPaneDocument> CreateDefaultDocumentInstanceAsync()
    {
        return CreateDefaultDocumentAsync();
    }

    protected override void OnDocumentRemoved(IPaneDocument document)
    {
        base.OnDocumentRemoved(document);

        if (document is not TableDataViewModel tableData || string.IsNullOrEmpty(tableData.Path))
        {
            return;
        }

        // Only stores the application created are scratch data. A store the user opened from disk has
        // to survive the tab being closed.
        if (tableData.IsTemporaryStore)
        {
            _tableStoreFiles.Delete(tableData.Path);
        }
    }

    protected override void OnDocumentsRestored()
    {
        // A store left behind by a run that never got to close its documents is only reachable through
        // the session, so it is cleaned up once it is known which stores are still open.
        _tableStoreFiles.PruneOrphaned(OpenStorePaths());
    }

    #endregion

    #region Methods

    private async Task<IPaneDocument> CreateDefaultDocumentAsync()
    {
        if (CreateNewDocumentInstance() is not TableDataViewModel tableData)
        {
            throw new InvalidOperationException("Failed to create default TableDataViewModel instance.");
        }

        await tableData.CreateNewStoreAsync("Example Table");

        return tableData;
    }

    private IEnumerable<string> OpenStorePaths()
    {
        return Documents
            .OfType<TableDataViewModel>()
            .Select(document => document.Path)
            .Where(path => !string.IsNullOrEmpty(path));
    }

    #endregion
}