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
using TableConverter.Interfaces;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.History;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.ViewModels.Documents;
using TableConverter.Extensions;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string JoinTable = "TableData.JoinTable";
}

/// <summary>
/// Brings the columns of another open table over for the rows that match it on the chosen key columns.
/// </summary>
/// <remarks>
/// The table is changed in place rather than added to, because bringing another table in is how a table is
/// made whole rather than how a new one is made. Every column of the other table that is not one of the keys
/// is added, and each row takes the values of the first row of the other table that matches it; a row that
/// matches nothing keeps nothing under the new columns. The whole of it is one step in the table's history.
/// </remarks>
public class JoinTableCommandHandler(
    ISukiDialogManager dialogManager,
    ISukiToastManager toastManager,
    ITableStoreDbContextFactory databaseContextFactory,
    ITableHistory history)
    : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.JoinTable,
        "Join Table",
        "Bring the columns of another open table over for the rows that match it on chosen columns.",
        "JoinTableIcon",
        canSetLoadingState: true,
        canSetLoadingOnWorkspace: true);

    public bool CanExecute(object? parameter, ICommandContext context)
    {
        return context.TryGetTableDocument(out var document)
               && OtherTables(context, document).Count > 0;
    }

    public async Task Execute(object? parameter, ICommandContext context)
    {
        if (!context.TryGetTableDocument(out var document))
        {
            context.Cancel("No table data document is selected.");
            return;
        }

        var others = OtherTables(context, document);

        if (others.Count == 0)
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Information)
                .WithTitle("Nothing To Join On")
                .WithContent("No other table is open to join the columns of onto this one.")
                .Queue();

            return;
        }

        // The table is picked first, because which of its columns can be matched on is only known once the
        // table itself is known.
        var tableList = CreateChoices(others.Select(table => table.Title), maxHeight: 250);
        tableList.SelectedIndex = 0;

        var confirmedTable = await dialogManager.CreateDialog()
            .WithTitle("Join Table")
            .WithContentList(
            [
                CreateLabelled("Table to join on", tableList),
            ])
            .Dismiss().ByClickingBackground()
            .WithYesNoResult("Next", "Cancel")
            .TryShowAsync();

        if (!confirmedTable)
        {
            return;
        }

        var other = others[Math.Clamp(tableList.SelectedIndex, 0, others.Count - 1)];

        var destinationColumns = await ReadColumnsAsync(document.Path);
        var sourceColumns = await ReadColumnsAsync(other.Path);

        if (destinationColumns.Count == 0 || sourceColumns.Count == 0)
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Information)
                .WithTitle("Nothing To Join On")
                .WithContent("Both tables need columns before they can be matched up.")
                .Queue();

            return;
        }

        // The keys are picked from each table in the order they sit in it, and the columns are paired up in
        // that order, so the first column chosen here is matched to the first one chosen there.
        var destinationList = CreateChoices(destinationColumns.Select(column => column.Name), maxHeight: 220);
        destinationList.SelectionMode = SelectionMode.Multiple;

        var sourceList = CreateChoices(sourceColumns.Select(column => column.Name), maxHeight: 220);
        sourceList.SelectionMode = SelectionMode.Multiple;

        var confirmedKeys = await dialogManager.CreateDialog()
            .WithTitle($"Join '{other.Title}' On")
            .WithContentList(
            [
                CreateLabelled("Columns in this table", destinationList),
                CreateLabelled($"Columns in '{other.Title}'", sourceList),
            ])
            .Dismiss().ByClickingBackground()
            .WithYesNoResult("Join", "Cancel")
            .TryShowAsync();

        if (!confirmedKeys)
        {
            return;
        }

        var destinationIndices = IndexesOf(destinationColumns.Select(column => column.Name).ToList(),
            ReadSelectedNames(destinationList));

        var sourceIndices = IndexesOf(sourceColumns.Select(column => column.Name).ToList(),
            ReadSelectedNames(sourceList));

        if (destinationIndices.Count == 0 || destinationIndices.Count != sourceIndices.Count)
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Information)
                .WithTitle("Nothing To Join On")
                .WithContent("Choose the same number of columns on each side before they can be matched up.")
                .Queue();

            return;
        }

        var matches = destinationIndices
            .Zip(sourceIndices, (destination, source) =>
                new ColumnMatch(destinationColumns[destination].Name, sourceColumns[source].Name))
            .ToList();

        TableJoinResult result;

        // The other table is read into a lookup once and this table is walked a batch at a time, so a join
        // costs as much memory as the table being joined on holds rather than as much as this one holds.
        await using (var sourceDb = await databaseContextFactory.CreateDbContextAsync(other.Path))
        await using (var destinationDb = await databaseContextFactory.CreateDbContextAsync(document.Path))
        {
            var source = TableStoreRowSource.Create(sourceDb);

            result = await TableStoreTableJoining.JoinAsync(
                destinationDb,
                history,
                document.Path,
                source,
                matches,
                $"Joined '{other.Title}' on {matches.Count} column(s)");
        }

        if (result.AddedColumns.Count == 0)
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Information)
                .WithTitle("Nothing To Bring Over")
                .WithContent($"'{other.Title}' holds no columns beyond the ones matched on.")
                .Queue();

            return;
        }

        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("Table Joined")
            .WithContent($"{result.MatchedRowCount} row(s) matched '{other.Title}'.")
            .Queue();

        // Columns were added to the table, so its columns are read again rather than only its rows.
        await document.InvalidateDataAsync();
    }

    /// <summary>
    /// The other open tables the columns can be joined from, which is every open table but this one.
    /// </summary>
    private static List<TableDataViewModel> OtherTables(ICommandContext context, TableDataViewModel? exclude)
    {
        if (context.Parent is not IWorkspaceEditor editor)
        {
            return [];
        }

        return
        [
            .. editor.Documents
                .OfType<TableDataViewModel>()
                .Where(table => !ReferenceEquals(table, exclude) && !string.IsNullOrEmpty(table.Path))
        ];
    }

    private async Task<List<ColumnEntity>> ReadColumnsAsync(string path)
    {
        await using var reader = await databaseContextFactory.CreateDbContextAsync(path);

        return await reader.Columns
            .AsNoTracking()
            .OrderBy(column => column.OrdinalPosition)
            .ToListAsync();
    }

    /// <summary>
    /// The places the chosen columns hold in a table, in the order they sit in it.
    /// </summary>
    private static List<int> IndexesOf(IReadOnlyList<string> names, IReadOnlyList<string> chosen)
    {
        var wanted = new HashSet<string>(chosen, StringComparer.Ordinal);
        var indices = new List<int>();

        for (var index = 0; index < names.Count; index++)
        {
            if (wanted.Contains(names[index]))
            {
                indices.Add(index);
            }
        }

        return indices;
    }

    /// <summary>
    /// Reads the names of the columns a multi-select picker is holding.
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
    /// Builds the picker a choice is made with.
    /// </summary>
    private static ListBox CreateChoices(IEnumerable<string> choices, double maxHeight = double.PositiveInfinity)
    {
        return new ListBox
        {
            ItemsSource = choices.ToList(),
            SelectionMode = SelectionMode.Single,
            MaxHeight = maxHeight,
            ItemsPanel = new FuncTemplate<Panel?>(() => new UniformGrid
            {
                Columns = 1,
                RowSpacing = 10,
                ColumnSpacing = 10,
            }),
        };
    }

    /// <summary>
    /// Puts a caption above a control so what it is asking for is not left to be guessed.
    /// </summary>
    private static StackPanel CreateLabelled(string caption, Control control)
    {
        return new StackPanel
        {
            Spacing = 5,
            Children =
            {
                new TextBlock { Text = caption },
                control,
            },
        };
    }
}
