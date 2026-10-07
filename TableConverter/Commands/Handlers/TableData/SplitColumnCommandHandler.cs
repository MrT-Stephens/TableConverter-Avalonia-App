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
    public const string SplitColumn = "TableData.SplitColumn";
}

/// <summary>
/// Splits the values of one column into several, putting the parts where the column that was read sat.
/// </summary>
/// <remarks>
/// The table is changed in place rather than added to, because parting a column is how a table is tidied up
/// rather than how a wider one is made: the column that was read is taken out, so what is left is the tidied
/// table rather than the table plus the parts. The whole of it is one step in the table's history, so a
/// split that turned out not to be wanted is taken back the way any other change is.
/// </remarks>
public class SplitColumnCommandHandler(
    ISukiDialogManager dialogManager,
    ISukiToastManager toastManager,
    ITableStoreDbContextFactory databaseContextFactory,
    ITableHistory history)
    : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.SplitColumn,
        "Split Column",
        "Split the values of one column into several, putting the parts where the column sat.",
        "SplitColumnIcon",
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

        if (columns.Count == 0)
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Information)
                .WithTitle("Nothing To Split")
                .WithContent("The table has no columns, so there is nothing to part.")
                .Queue();

            return;
        }

        var columnList = CreateChoices(columns.Select(column => column.Name), maxHeight: 250);
        columnList.SelectedIndex = 0;

        var separatorBox = new TextBox { Text = ",", PlaceholderText = "Separator" };
        var partsBox = new TextBox { Text = "Part 1, Part 2", PlaceholderText = "Part names, separated by commas" };
        var removeSourceBox = new CheckBox { Content = "Take out the column that was read", IsChecked = true };

        var confirmed = await dialogManager.CreateDialog()
            .WithTitle("Split Column")
            .WithContentList(
            [
                CreateLabelled("Column to split", columnList),
                CreateLabelled("Separator", separatorBox),
                CreateLabelled("Part names", partsBox),
                removeSourceBox,
            ])
            .Dismiss().ByClickingBackground()
            .WithYesNoResult("Split", "Cancel")
            .TryShowAsync();

        if (!confirmed)
        {
            return;
        }

        var columnName = columnList.SelectedItem as string;

        var source = columns.FirstOrDefault(candidate =>
            string.Equals(candidate.Name, columnName, StringComparison.Ordinal));

        if (source is null)
        {
            context.Cancel("No column was selected to split.");
            return;
        }

        var separator = separatorBox.Text ?? string.Empty;

        if (string.IsNullOrEmpty(separator))
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Information)
                .WithTitle("No Separator Given")
                .WithContent("A separator is needed to say what each value is split on.")
                .Queue();

            return;
        }

        var partNames = ReadPartNames(partsBox.Text);

        if (partNames.Count < 2)
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Information)
                .WithTitle("Nothing To Split Into")
                .WithContent("Give at least two part names, separated by commas, for the parts to be written under.")
                .Queue();

            return;
        }

        await using (var db = await databaseContextFactory.CreateDbContextAsync(document.Path))
        {
            await TableStoreColumnJoining.SplitColumnAsync(
                db,
                history,
                document.Path,
                source.Id,
                separator,
                partNames,
                removeSourceBox.IsChecked == true,
                $"Split '{source.Name}' into {partNames.Count} columns");
        }

        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("Column Split")
            .WithContent($"'{source.Name}' was parted into {partNames.Count} columns.")
            .Queue();

        // The column that was read is taken out and the parts take its place, so the table's columns are read
        // again rather than only its rows.
        await document.InvalidateDataAsync();
    }

    /// <summary>
    /// Reads the part names out of what was typed, which are written as a comma separated list.
    /// </summary>
    private static IReadOnlyList<string> ReadPartNames(string? text)
    {
        return
        [
            .. (text ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(name => name.Length > 0)
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

