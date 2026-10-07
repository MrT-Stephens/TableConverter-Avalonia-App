using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Extensions;
using TableConverter.Commands.Interfaces;
using TableConverter.Interfaces;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.History;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.ViewModels.Documents;
using TableConverter.Extensions;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string MergeTable = "TableData.MergeTable";
}

/// <summary>
/// Brings the rows of another open table into this one, adding them after the rows it already holds.
/// </summary>
/// <remarks>
/// The table is changed in place rather than added to, because bringing another table in is how a table is
/// made whole rather than how a new one is made. The columns of the other table are matched up by name, so a
/// column the table already holds has the values written under the column that is already there, and a column
/// it does not yet hold is added. The whole of it is one step in the table's history.
/// </remarks>
public class MergeTableCommandHandler(
    ISukiDialogManager dialogManager,
    ISukiToastManager toastManager,
    ITableStoreDbContextFactory databaseContextFactory,
    ITableHistory history)
    : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.MergeTable,
        "Bring In Table",
        "Add the rows of another open table after the rows this one already holds.",
        "MergeTableIcon",
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
                .WithTitle("Nothing To Bring In")
                .WithContent("No other table is open to bring the rows of into this one.")
                .Queue();

            return;
        }

        var tableList = CreateChoices(others.Select(table => table.Title), maxHeight: 250);
        tableList.SelectedIndex = 0;

        var confirmed = await dialogManager.CreateDialog()
            .WithTitle("Bring In Table")
            .WithContentList(
            [
                CreateLabelled("Table to bring in", tableList),
            ])
            .Dismiss().ByClickingBackground()
            .WithYesNoResult("Bring In", "Cancel")
            .TryShowAsync();

        if (!confirmed)
        {
            return;
        }

        var other = others[Math.Clamp(tableList.SelectedIndex, 0, others.Count - 1)];

        TableMergeResult result;

        // The other table is read a row at a time rather than held in memory, so a table of any size costs one
        // batch to bring in rather than the whole of it.
        await using (var sourceDb = await databaseContextFactory.CreateDbContextAsync(other.Path))
        await using (var destinationDb = await databaseContextFactory.CreateDbContextAsync(document.Path))
        {
            var source = TableStoreRowSource.Create(sourceDb);

            result = await TableStoreTableJoining.AppendAsync(
                destinationDb,
                history,
                document.Path,
                source,
                $"Brought the rows of '{other.Title}' in");
        }

        if (result.AppendedRowCount == 0)
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Information)
                .WithTitle("Nothing To Bring In")
                .WithContent($"'{other.Title}' had no rows to add.")
                .Queue();

            return;
        }

        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("Table Brought In")
            .WithContent($"Added {result.AppendedRowCount} row(s) from '{other.Title}'.")
            .Queue();

        // Rows were added and the table may have gained a column, so the whole table is read again.
        await document.InvalidateDataAsync();
    }

    /// <summary>
    /// The other open tables the rows can be brought in from, which is every open table but this one.
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

    /// <summary>
    /// Builds the picker a choice is made with.
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
