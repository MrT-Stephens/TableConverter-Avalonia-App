using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Interfaces;
using TableConverter.ViewModels.Base;
using TableConverter.ViewModels.Documents;
using TableConverter.ViewModels.Workspaces;

namespace TableConverter.ViewModels.Tools;

/// <summary>
///     Shows what the selected table holds that does not read as the type of the column it is under, so a
///     value that was typed into a column of another type can be found and put right.
/// </summary>
/// <remarks>
///     <para>
///         A column's type is metadata: nothing is rejected when a value is written, so a value only turns
///         out not to read as its type when it is looked at. This panel is that look, and it reads the table
///         rather than keeping a copy of it, so it always describes the table as it stands.
///     </para>
///     <para>
///         The check is run when the panel is brought to the front and when the selected document changes,
///         rather than on every edit: a table can be large, and reading it after each keystroke would cost
///         more than the answer is worth. The panel's own button runs it again on demand.
///     </para>
/// </remarks>
public partial class TableValidationViewModel : BaseScopedPaneToolViewModel<TableWorkspaceEditorViewModel>
{
    #region Properties

    /// <summary>
    ///     The columns that hold a value that does not read as their type, in table order. A column whose
    ///     values all read as its type is not listed, so the panel only shows what has to be worked on.
    /// </summary>
    [ObservableProperty] private ObservableCollection<ValidationColumnViewModel> _Columns;

    /// <summary>
    ///     Whether a table has been read, which is what tells a table with nothing wrong with it apart from
    ///     no table being open at all.
    /// </summary>
    [ObservableProperty] private bool _HasReport;

    /// <summary>
    ///     Whether the table that was read holds anything that does not read as its column's type.
    /// </summary>
    [ObservableProperty] private bool _HasProblems;

    /// <summary>
    ///     What the check found, in a form short enough to sit above the list.
    /// </summary>
    [ObservableProperty] private string _Summary;

    /// <summary>
    ///     Whether a check is running, which is what keeps the panel's button from starting a second one.
    /// </summary>
    [ObservableProperty] private bool _IsChecking;

    private readonly ITableStoreDbContextFactory _dbContextFactory;
    private readonly ILogger<TableValidationViewModel> _logger;

    /// <summary>
    ///     The store being checked, or empty when the selected document is not a table.
    /// </summary>
    private string _path = string.Empty;

    /// <summary>
    ///     How many times the table has been read.
    /// </summary>
    /// <remarks>
    ///     A check told to run while an earlier one is still on its way would otherwise be overwritten by,
    ///     or interleaved with, the reading that no longer describes the table, so a reading is only applied
    ///     while it is still the newest one asked for. It is touched from whichever thread the read
    ///     finished on as well as from the UI thread, so it is only ever moved atomically.
    /// </remarks>
    private int _generation;

    #endregion

    #region Constructors

    public TableValidationViewModel(
        ICommandManager commandManager,
        IEventManager eventManager,
        ISukiDialogManager dialogManager,
        ISukiToastManager toastManager,
        ITableStoreDbContextFactory dbContextFactory,
        ILogger<TableValidationViewModel> logger)
        : base(commandManager, eventManager, dialogManager, toastManager, "Validation", false)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        Columns = [];
        Summary = string.Empty;
    }

    #endregion

    #region Overrides

    /// <summary>
    ///     Runs the check again, on the table the panel is showing or the one it was just switched to.
    /// </summary>
    [RelayCommand]
    private async Task CheckAsync()
    {
        var path = _path;
        var generation = Interlocked.Increment(ref _generation);

        // The check is started from the UI - the panel's button, or the panel being brought to the front -
        // so the state that says one is running is set here rather than marshalled to the UI thread.
        IsChecking = true;

        TableValidationReport? report = null;

        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                await using var dbContext = await _dbContextFactory
                    .CreateDbContextAsync(path).ConfigureAwait(false);

                report = await TableStoreValidation.ReadAsync(dbContext).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                // A table that cannot be read is shown as nothing rather than being allowed to take the
                // application down; whatever stopped it being read reports itself in its own way.
                _logger.LogWarning(exception, "The table store '{Path}' could not be validated.", path);
            }
        }

        Apply(report, generation);
    }

    public override void OnActivate()
    {
        base.OnActivate();

        // Cells are edited straight in the table's own grid, which this tool cannot see. Reading the table
        // again when the tool is brought back to the front keeps the report in step with what was typed.
        CheckCommand.Execute(null);
    }

    protected override void OnSelectedDocumentChanged(
        IWorkspace workspace,
        IPaneDocument? oldDocument,
        IPaneDocument? newDocument)
    {
        base.OnSelectedDocumentChanged(workspace, oldDocument, newDocument);

        if (Workspace != workspace)
        {
            return;
        }

        // A store is assigned to a document after it was selected, which is how a restored document loads,
        // so the path is followed rather than the document itself.
        var path = newDocument is TableDataViewModel tableData ? tableData.Path : string.Empty;

        if (string.Equals(path, _path, StringComparison.Ordinal))
        {
            return;
        }

        _path = path;

        CheckCommand.Execute(null);
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Hands the report to the panel, on the UI thread, unless a later check has already replaced it.
    /// </summary>
    private void Apply(TableValidationReport? report, int generation)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            Build(report, generation);
            return;
        }

        Dispatcher.UIThread.Post(() => Build(report, generation));
    }

    private void Build(TableValidationReport? report, int generation)
    {
        // A reading that is no longer the newest one asked for describes a table the panel has already moved
        // past, so it is dropped rather than allowed to put back what it read.
        if (IsDisposed || generation != Volatile.Read(ref _generation))
        {
            return;
        }

        IsChecking = false;
        Columns.Clear();

        if (report is null)
        {
            HasReport = false;
            HasProblems = false;
            Summary = string.Empty;
            return;
        }

        // The cells are grouped once rather than searched for each column, so a table with many columns does
        // not cost a walk of every cell for each of them.
        var cellsByColumn = report.Cells
            .GroupBy(cell => cell.ColumnId)
            .ToDictionary(group => group.Key, group => group.ToList());

        foreach (var column in report.Columns)
        {
            var cells = new ObservableCollection<ValidationCellViewModel>(
                (cellsByColumn.GetValueOrDefault(column.ColumnId) ?? [])
                .Select(cell => new ValidationCellViewModel(cell.RowPosition, cell.Value, JumpToCellAsync)));

            Columns.Add(new ValidationColumnViewModel(column, cells));
        }

        HasReport = true;
        HasProblems = report.InvalidCellCount > 0;
        Summary = BuildSummary(report);
    }

    /// <summary>
    ///     Puts the grid on the row that holds one of the faulty values, so the value can be seen where it
    ///     lives rather than only in the report.
    /// </summary>
    private Task JumpToCellAsync(ValidationCellViewModel cell)
    {
        if (Workspace?.SelectedDocument is not TableDataViewModel document
            || !string.Equals(document.Path, _path, StringComparison.Ordinal))
        {
            return Task.CompletedTask;
        }

        // The report counts rows from one; the grid counts them from zero.
        return document.SelectCellAsync(cell.RowPosition - 1);
    }

    private static string BuildSummary(TableValidationReport report)
    {
        if (report.RowCount == 0 || report.ColumnCount == 0)
        {
            return "This table is empty, so there is nothing to check.";
        }

        if (report.InvalidCellCount == 0)
        {
            return $"Every value in {report.RowCount:N0} "
                   + $"{(report.RowCount == 1 ? "row" : "rows")} reads as the type of its column.";
        }

        var noun = report.InvalidCellCount == 1 ? "value doesn't" : "values don't";
        var columns = report.Columns.Count;
        var truncated = report.IsTruncated ? " The list below shows the first few of them." : string.Empty;

        return $"{report.InvalidCellCount:N0} {noun} read as their column's type across {columns:N0} "
               + $"{(columns == 1 ? "column" : "columns")}.{truncated}";
    }

    #endregion
}
