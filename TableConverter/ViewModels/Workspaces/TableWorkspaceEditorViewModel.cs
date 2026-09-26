using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
        
        MainCommands.Add(this[TableDataCommandNames.NewFile]);
        MainCommands.Add(this[TableDataCommandNames.ImportFile]);
        MainCommands.Add(this[TableDataCommandNames.ExportFile]);
        MainCommands.Add(this[TableDataCommandNames.Search]);
        MainCommands.Add(this[TableDataCommandNames.OpenFile]);

        // The table tools are commands too, so the same actions are offered by the workspace menu and by the
        // table utilities tool, with one implementation behind both.
        MainCommands.Add(this[TableDataCommandNames.AddRow]);
        MainCommands.Add(this[TableDataCommandNames.DeleteRows]);
        MainCommands.Add(this[TableDataCommandNames.TrimWhitespace]);
        MainCommands.Add(this[TableDataCommandNames.RemoveDuplicateRows]);
        MainCommands.Add(this[TableDataCommandNames.TransposeClockwise]);
        MainCommands.Add(this[TableDataCommandNames.TransposeCounterClockwise]);
        MainCommands.Add(this[TableDataCommandNames.SortByColumn]);
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
