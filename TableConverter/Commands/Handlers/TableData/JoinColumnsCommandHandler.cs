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
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.History;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Extensions;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string JoinColumns = "TableData.JoinColumns";
}

/// <summary>
/// Joins the values of several columns of the table into one, putting the result where the first of the
/// columns that were read sat.
/// </summary>
/// <remarks>
/// The table is changed in place rather than added to, because joining columns is how a table is tidied up
/// rather than how a wider one is made: the columns that were read are taken out, so what is left is the
/// tidied table rather than the table plus the tidied columns. The whole of it is one step in the table's
/// history, so a join that turned out not to be wanted is taken back the way any other change is.
/// </remarks>
public class JoinColumnsCommandHandler(
    ISukiDialogManager dialogManager,
    ISukiToastManager toastManager,
    ITableStoreDbContextFactory databaseContextFactory,
    ITableHistory history)
    : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.JoinColumns,
        "Join Columns",
        "Join the values of several columns into one, putting the result where the first of them sat.",
        "JoinColumnsIcon",
        canSetLoadingState: true,
        canSetLoadingOnWorkspace: true);

    public bool CanExecute(object? parameter, ICommandContext context)
    {
        return context.TryGetTableDocument(out _);
    }

    public async Task Execute(object? parameter, ICommandContext context)
    {
        if (!context.TryGetTableDocument(out var document))
        {
            context.Cancel("No table data document is selected.");
            return;
        }

        List<ColumnEntity> columns;

        await using (var reader = await databaseContextFactory.CreateDbContextAsync(document.Path))
        {
            columns = await reader.Columns
                .AsNoTracking()
                .OrderBy(column => column.OrdinalPosition)
                .ToListAsync();
        }

        if (columns.Count < 2)
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Information)
                .WithTitle("Nothing To Join")
                .WithContent("The table needs at least two columns before any of them can be joined together.")
                .Queue();

            return;
        }

        // The columns are chosen from the table's own order, and the values are joined in that order, so
        // the result reads the way the table does rather than in the order the picker happens to hold.
        var columnList = CreateChoices(columns.Select(column => column.Name), maxHeight: 250);
        columnList.SelectionMode = SelectionMode.Multiple;

        var nameBox = new TextBox { PlaceholderText = "New column name" };
        var separatorBox = new TextBox { Text = ", ", PlaceholderText = "Separator" };
        var skipBlankBox = new CheckBox { Content = "Leave out the cells that are empty", IsChecked = true };
        var removeSourceBox = new CheckBox { Content = "Take out the columns that were read", IsChecked = true };

        var confirmed = await dialogManager.CreateDialog()
            .WithTitle("Join Columns")
            .WithContentList(
            [
                CreateLabelled("Columns to join", columnList),
                CreateLabelled("New column name", nameBox),
                CreateLabelled("Separator", separatorBox),
                skipBlankBox,
                removeSourceBox,
            ])
            .Dismiss().ByClickingBackground()
            .WithYesNoResult("Join", "Cancel")
            .TryShowAsync();

        if (!confirmed)
        {
            return;
        }

        var chosen = ReadSelectedNames(columnList);
        var resultName = nameBox.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(resultName))
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Information)
                .WithTitle("No Name Given")
                .WithContent("The joined column has to be given a name.")
                .Queue();

            return;
        }

        var sourceIds = columns
            .Where(column => chosen.Contains(column.Name))
            .Select(column => column.Id)
            .ToList();

        if (sourceIds.Count < 2)
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Information)
                .WithTitle("Nothing To Join")
                .WithContent("At least two columns have to be chosen before they can be joined together.")
                .Queue();

            return;
        }

        await using (var db = await databaseContextFactory.CreateDbContextAsync(document.Path))
        {
            await TableStoreColumnJoining.JoinColumnsAsync(
                db,
                history,
                document.Path,
                sourceIds,
                resultName,
                separatorBox.Text ?? string.Empty,
                skipBlankBox.IsChecked == true,
                removeSourceBox.IsChecked == true,
                $"Joined {sourceIds.Count} columns into '{resultName}'");
        }

        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("Columns Joined")
            .WithContent($"The chosen values were joined into '{resultName}'.")
            .Queue();

        // A column that was read is taken out and the joined one takes its place, so the table's columns are
        // read again rather than only its rows.
        await document.InvalidateDataAsync();
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
                Columns = 2,
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

