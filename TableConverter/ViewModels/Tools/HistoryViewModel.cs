using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.Handlers.TableData;
using TableConverter.Commands.Interfaces;
using TableConverter.Extensions;
using TableConverter.Interfaces;
using TableConverter.Utilities.Database.History;
using TableConverter.Utilities.Extensions;
using TableConverter.Utilities.Interfaces;
using TableConverter.ViewModels.Base;
using TableConverter.ViewModels.Documents;
using TableConverter.ViewModels.Workspaces;

namespace TableConverter.ViewModels.Tools;

/// <summary>
///     Shows the changes made to the selected table, oldest first, with the point the table is currently
///     sitting at marked between the changes that are done and the changes that have been taken back.
/// </summary>
/// <remarks>
///     The history belongs to the store file rather than to this panel, so the panel only ever reads it and
///     never keeps a copy of its own: a change made by the grid, by a menu command or by another panel is
///     picked up through the history's own change notification.
/// </remarks>
public partial class HistoryViewModel : BaseScopedPaneToolViewModel<TableWorkspaceEditorViewModel>
{
    #region Properties

    /// <summary>
    ///     The steps that are currently done to the table, oldest first. The table is sitting immediately
    ///     after the last of them.
    /// </summary>
    [ObservableProperty] private ObservableCollection<HistoryEntryViewModel> _AppliedEntries;

    /// <summary>
    ///     The steps that have been taken back, oldest first, and which redoing puts in place again in the
    ///     order they are listed.
    /// </summary>
    [ObservableProperty] private ObservableCollection<HistoryEntryViewModel> _UndoneEntries;

    [ObservableProperty] private ObservableCollection<ICommandInstance> _HistoryCommands;

    /// <summary>
    ///     Whether there is anything at all to list, which is what decides between the history and the
    ///     message shown for a table that has not been changed yet.
    /// </summary>
    [ObservableProperty] private bool _HasEntries;

    private readonly ITableHistory _history;
    private readonly ILogger<HistoryViewModel> _logger;

    /// <summary>
    ///     The store whose history is being shown, or empty when the selected document is not a table.
    /// </summary>
    private string _path = string.Empty;

    /// <summary>
    ///     How many times the history has been read.
    /// </summary>
    /// <remarks>
    ///     A history told to change while an earlier reading is still on its way would otherwise be
    ///     overwritten by, or interleaved with, the reading that no longer describes it, so a reading is
    ///     only applied while it is still the newest one asked for. It is touched from whichever thread
    ///     reported the change as well as from the UI thread, so it is only ever moved atomically.
    /// </remarks>
    private int _RefreshGeneration;

    #endregion

    #region Constructors

    public HistoryViewModel(
        ICommandManager commandManager,
        IEventManager eventManager,
        ISukiDialogManager dialogManager,
        ISukiToastManager toastManager,
        ITableHistory history,
        ILogger<HistoryViewModel> logger)
        : base(commandManager, eventManager, dialogManager, toastManager, "History", false)
    {
        _history = history;
        _logger = logger;
        AppliedEntries = [];
        UndoneEntries = [];
        HistoryCommands = [];
    }

    #endregion

    #region Overrides

    public override void Initialise()
    {
        base.Initialise();

        // Undo, redo and clearing are commands like any other, so the panel and the workspace menu share one
        // implementation of each and the panel's buttons are gated exactly as the menu items are.
        HistoryCommands.Add(this[TableDataCommandNames.Undo]);
        HistoryCommands.Add(this[TableDataCommandNames.Redo]);
        HistoryCommands.Add(this[TableDataCommandNames.ClearHistory]);

        // The history says when it changes rather than anyone who changes it having to know this panel
        // exists, so the panel follows the table however it was changed.
        _eventRegistrar.RegisterEvent<EventHandler<HistoryChangedEventArgs>>(
            action => _history.Changed += action,
            action => _history.Changed -= action,
            null,
            OnHistoryChanged);
    }

    protected override void OnSelectedDocumentChanged(IWorkspace workspace,
        IPaneDocument? oldDocument, IPaneDocument? newDocument)
    {
        base.OnSelectedDocumentChanged(workspace, oldDocument, newDocument);

        if (Workspace != workspace)
        {
            return;
        }

        // A history belongs to the store rather than to the document, so the path is what is followed. A
        // document whose store is assigned after it was selected - which is how a restored document loads -
        // is therefore picked up as well.
        var path = newDocument is TableDataViewModel tableData ? tableData.Path : string.Empty;

        if (string.Equals(path, _path, StringComparison.Ordinal))
        {
            return;
        }

        _path = path;

        RefreshAsync().FireAndForget();
    }

    #endregion

    #region Methods

    private void OnHistoryChanged(object? sender, HistoryChangedEventArgs args)
    {
        // Every store reports through the one history service, so a change to a store that is not being
        // shown is none of this panel's business.
        if (!string.Equals(args.Path, _path, StringComparison.Ordinal))
        {
            return;
        }

        RefreshAsync().FireAndForget();
    }

    /// <summary>
    ///     Reads the history of the store being shown and lays it out around the point the table is at.
    /// </summary>
    /// <remarks>
    ///     Reading the history is not rendering work, so it is done on whichever thread the change arrived
    ///     on; the lists are UI state and are only ever changed on the UI thread.
    /// </remarks>
    private async Task RefreshAsync()
    {
        var path = _path;
        var generation = Interlocked.Increment(ref _RefreshGeneration);

        IReadOnlyList<HistoryEntry> entries;

        try
        {
            entries = string.IsNullOrEmpty(path)
                ? []
                : await _history.GetEntriesAsync(path).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            // A history that cannot be read is shown as nothing rather than being allowed to take the
            // application down; whatever stopped it being read reports itself in its own way.
            _logger.LogWarning(exception, "The history of the table store '{Path}' could not be read.", path);

            entries = [];
        }

        ApplyEntries(generation, entries);
    }

    private void ApplyEntries(int generation, IReadOnlyList<HistoryEntry> entries)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            BuildEntries(generation, entries);
            return;
        }

        Dispatcher.UIThread.Post(() => BuildEntries(generation, entries));
    }

    /// <summary>
    ///     Splits the history into the steps that are done to the table and the steps that have been taken
    ///     back, so the two lists can be drawn either side of the point the table is sitting at.
    /// </summary>
    private void BuildEntries(int generation, IReadOnlyList<HistoryEntry> entries)
    {
        // A reading that is no longer the newest one asked for describes a history the panel has already
        // moved past, so it is dropped rather than allowed to put back what it read.
        if (IsDisposed || generation != Volatile.Read(ref _RefreshGeneration))
        {
            return;
        }

        AppliedEntries.Clear();
        UndoneEntries.Clear();

        foreach (var entry in entries)
        {
            var viewModel = new HistoryEntryViewModel(entry);

            if (entry.IsApplied)
            {
                AppliedEntries.Add(viewModel);
            }
            else
            {
                UndoneEntries.Add(viewModel);
            }
        }

        HasEntries = AppliedEntries.Count > 0 || UndoneEntries.Count > 0;
    }

    #endregion
}

