using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Microsoft.EntityFrameworkCore;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Extensions;
using TableConverter.Commands.Interfaces;
using TableConverter.Extensions;
using TableConverter.Interfaces;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.ViewModels.Documents;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string PivotTable = "TableData.PivotTable";
}

/// <summary>
///     Boils the table down to one row per group, working out a total for each, and opens the summary as a
///     new document.
/// </summary>
/// <remarks>
///     The table is left as it was: the summary is a table of its own rather than a rearranging of the one
///     it was worked out from, so the answer can be read, sorted or exported without losing the detail it
///     was summarised from.
/// </remarks>
public class PivotTableCommandHandler(
    ISukiDialogManager dialogManager,
    ISukiToastManager toastManager,
    ITableStoreDbContextFactory databaseContextFactory)
    : ICommandHandlerAsync
{
    /// <summary>
    ///     The ways a group of rows can be boiled down, in the order they are offered. Counting the rows is
    ///     told apart from counting the values, because a count over a column leaves its empty cells out.
    /// </summary>
    private static readonly PivotAggregate[] Aggregates =
    [
        new("Count the rows", AggregateKind.Count, false),
        new("Count the values", AggregateKind.Count, true),
        new("Sum", AggregateKind.Sum, true),
        new("Average", AggregateKind.Average, true),
        new("Minimum", AggregateKind.Minimum, true),
        new("Maximum", AggregateKind.Maximum, true),
    ];

    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.PivotTable,
        "Summarise Table",
        "Group the rows of the table and work out a total for each group, opening the result as a new table.",
        "PivotIcon",
        ["Ctrl+Shift+P"],
        canSetLoadingState: true,
        canSetLoadingOnWorkspace: true);

    public bool CanExecute(object? parameter, ICommandContext context)
    {
        return context.TryGetTableDocument(out _);
    }

    public async Task Execute(object? parameter, ICommandContext context)
    {
        if (!context.TryGetTableDocument(out var source))
        {
            context.Cancel("No table data document is selected.");
            return;
        }

        // The summary is a table of its own, so it is opened by the workspace the source table sits in.
        if (context.Parent is not IWorkspaceEditor editor)
        {
            context.Cancel("The table cannot be summarised outside a workspace.");
            return;
        }

        List<ColumnEntity> columns;

        await using (var reader = await databaseContextFactory.CreateDbContextAsync(source.Path))
        {
            columns = await reader.Columns
                .AsNoTracking()
                .OrderBy(column => column.OrdinalPosition)
                .ToListAsync();
        }

        if (columns.Count == 0)
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Information)
                .WithTitle("Nothing To Summarise")
                .WithContent("The table has no columns, so there is nothing to group its rows by.")
                .Queue();

            return;
        }

        var settings = await AskForSettingsAsync(columns);

        if (settings is null)
        {
            return;
        }

        if (settings.KeyColumnNames.Count == 0)
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Information)
                .WithTitle("Nothing To Group By")
                .WithContent("No column was chosen to group the rows by.")
                .Queue();

            return;
        }

        var valueColumnName = settings.Aggregate.UsesValue ? settings.ValueColumnName : null;

        if (settings.Aggregate.UsesValue && string.IsNullOrWhiteSpace(valueColumnName))
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Information)
                .WithTitle("Nothing To Summarise")
                .WithContent("No column was chosen to work the total out from.")
                .Queue();

            return;
        }

        var title = $"Summary by {string.Join(", ", settings.KeyColumnNames)}";
        var document = (TableDataViewModel)editor.CreateNewDocumentInstance();
        var destinationPath = document.ReserveStorePath();

        try
        {
            // The rows are read from the source and written straight into the new store, one group at a
            // time, so the summary never holds the table it was worked out from in memory.
            await using (var sourceDb = await databaseContextFactory.CreateDbContextAsync(source.Path))
            await using (var destinationDb = await databaseContextFactory.CreateDbContextAsync(destinationPath))
            {
                var rows = TableStoreRowSource.Create(sourceDb);

                await using var sink = TableStoreRowSink.Create(destinationDb);

                await TablePivot.PivotAsync(
                    rows, sink, settings.KeyColumnNames, valueColumnName, settings.Aggregate.Kind);
            }
        }
        catch
        {
            // A document that never made it into the workspace is not shown, and its store is scratch data
            // nobody will ever reach, so both are let go of rather than left behind.
            document.Dispose();
            throw;
        }

        await document.LoadAsync(destinationPath, title, isTemporaryStore: true);

        editor.AddDocument(document);
        editor.SelectedDocument = document;

        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("Table Summarised")
            .WithContent($"The summary of '{source.Title}' was opened as a new table.")
            .Queue();
    }

    /// <summary>
    ///     Asks which columns the rows are grouped by, how they are summarised and which column is read.
    /// </summary>
    /// <returns>The settings that were asked for, or <see langword="null" /> when the dialog was dismissed.</returns>
    private async Task<PivotSettings?> AskForSettingsAsync(IReadOnlyList<ColumnEntity> columns)
    {
        var names = columns.Select(column => column.Name).ToList();

        var keyList = CreateChoices(names, maxHeight: 200);
        keyList.SelectionMode = SelectionMode.Multiple;

        var aggregateList = CreateChoices(Aggregates.Select(aggregate => aggregate.Label));
        aggregateList.SelectedIndex = 0;

        var valueList = CreateChoices(names, maxHeight: 200);
        valueList.SelectedIndex = 0;

        // The column a total is worked out from only means anything when the rows are not simply counted,
        // so it is kept out of the way while counting the rows is what is being asked for.
        var valuePanel = Labelled("Value column", valueList);

        void ShowValueFor(int index)
        {
            valuePanel.IsVisible = Aggregates[Math.Clamp(index, 0, Aggregates.Length - 1)].UsesValue;
        }

        aggregateList.SelectionChanged += (_, _) => ShowValueFor(aggregateList.SelectedIndex);

        ShowValueFor(aggregateList.SelectedIndex);

        var confirmed = await dialogManager.CreateDialog()
            .WithTitle("Summarise Table")
            .WithContentList(
            [
                Labelled("Group by", keyList),
                Labelled("Summarise", aggregateList),
                valuePanel,
            ])
            .Dismiss().ByClickingBackground()
            .WithYesNoResult("Summarise", "Cancel")
            .TryShowAsync();

        if (!confirmed)
        {
            return null;
        }

        var aggregate = Aggregates[Math.Clamp(aggregateList.SelectedIndex, 0, Aggregates.Length - 1)];

        return new PivotSettings(aggregate, ReadSelectedNames(keyList), valueList.SelectedItem as string ?? string.Empty);
    }

    /// <summary>
    ///     Reads the names of the columns the multi-select list is holding.
    /// </summary>
    private static IReadOnlyList<string> ReadSelectedNames(ListBox list)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);

        if (list.SelectedItems is { } selected)
        {
            foreach (var item in selected)
            {
                if (item is string name)
                {
                    names.Add(name);
                }
            }
        }

        return [.. names];
    }

    /// <summary>
    ///     Builds the picker a choice is made with.
    /// </summary>
    private static ListBox CreateChoices(IEnumerable<string> choices, double maxHeight = double.PositiveInfinity)
    {
        return new ListBox
        {
            ItemsSource = choices.ToList(),
            SelectionMode = SelectionMode.AlwaysSelected | SelectionMode.Single,
            MaxHeight = maxHeight,
            ItemsPanel = new FuncTemplate<Panel?>(() => new UniformGrid
            {
                Columns = 2,
                RowSpacing = 10,
                ColumnSpacing = 10,
            }),
        };
    }

    /// <summary>
    ///     Puts a caption above a picker so what it is asking for is not left to be guessed.
    /// </summary>
    private static StackPanel Labelled(string caption, Control picker)
    {
        return new StackPanel
        {
            Spacing = 5,
            Children =
            {
                new TextBlock { Text = caption },
                picker,
            },
        };
    }

    /// <summary>
    ///     One of the ways a group of rows can be summarised, and whether it reads a column to do it.
    /// </summary>
    private sealed record PivotAggregate(string Label, AggregateKind Kind, bool UsesValue);

    /// <summary>
    ///     What was asked for in the dialog, in a form that can be handed to the pivot.
    /// </summary>
    private sealed record PivotSettings(
        PivotAggregate Aggregate,
        IReadOnlyList<string> KeyColumnNames,
        string ValueColumnName);
}

