using System;
using System.Collections.Generic;
using System.Globalization;
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
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.History;
using TableConverter.Utilities.Database.Interfaces;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.Utilities.Models;
using TableConverter.ViewModels.Tools;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string AddComputedColumn = "TableData.AddComputedColumn";
}

/// <summary>
///     Adds a column whose values are worked out from the values a row already holds.
/// </summary>
/// <remarks>
///     The operation, the columns it reads and the way it is set up are all asked for in one dialog, because
///     they only mean anything together: a separator with nothing to split is no operation at all.
/// </remarks>
public class AddComputedColumnCommandHandler(
    ISukiDialogManager dialogManager,
    ISukiToastManager toastManager,
    ITableStoreDbContextFactory databaseContextFactory,
    ITableHistory history)
    : ICommandHandlerAsync
{
    /// <summary>
    ///     The operations a derived column can be built from, in the order they are offered.
    /// </summary>
    private static readonly (string Label, ComputedOperation Operation)[] Operations =
    [
        ("Join values together", ComputedOperation.Concatenate),
        ("Split a value into parts", ComputedOperation.Split),
        ("Take characters out of a value", ComputedOperation.ExtractCharacters),
        ("Take a match out of a value", ComputedOperation.ExtractPattern),
        ("Work out an amount", ComputedOperation.Calculate),
        ("Take a part out of a date", ComputedOperation.DatePart),
    ];

    private static readonly string[] MathOperatorLabels = ["Add", "Subtract", "Multiply", "Divide"];

    private static readonly string[] DatePartLabels =
    [
        "Year", "Month", "Day", "Hour", "Minute", "Second", "Day of week", "Day of year", "Quarter",
        "Month name", "Day name",
    ];

    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.AddComputedColumn,
        "Add Derived Column",
        "Add a column whose values are worked out from the values a row already holds.",
        "MagicWandIcon",
        ["Ctrl+Shift+D"],
        canSetLoadingState: true,
        canSetLoadingOnWorkspace: true);

    public bool CanExecute(object? parameter, ICommandContext context)
    {
        return context.TryGetSelectedItem<TableColumnsEditorViewModel>(out var viewModel)
               && !string.IsNullOrEmpty(viewModel.DataSource.Path);
    }

    public async Task Execute(object? parameter, ICommandContext context)
    {
        if (!context.TryGetSelectedItem<TableColumnsEditorViewModel>(out var viewModel))
        {
            context.Cancel("No table columns editor found.");
            return;
        }

        var path = viewModel.DataSource.Path;

        if (string.IsNullOrEmpty(path))
        {
            context.Cancel("No table is open to add a column to.");
            return;
        }

        List<ColumnEntity> columns;

        await using (var reader = await databaseContextFactory.CreateDbContextAsync(path))
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
                .WithTitle("Nothing To Derive From")
                .WithContent("The table has no columns, so there are no values to work a new column out from.")
                .Queue();

            return;
        }

        var settings = await AskForSettingsAsync(columns);

        if (settings is null)
        {
            return;
        }

        var definition = settings.BuildDefinition(columns);

        if (definition is null)
        {
            return;
        }

        var sourceIds = settings.ResolveSourceColumnIds(columns);

        if (sourceIds.Count == 0)
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Information)
                .WithTitle("Nothing To Derive From")
                .WithContent("No column was chosen to read the values from.")
                .Queue();

            return;
        }

        IReadOnlyList<ColumnEntity> added;

        await using (var db = await databaseContextFactory.CreateDbContextAsync(path))
        {
            added = await TableStoreComputedColumns.AddAsync(
                db,
                history,
                path,
                sourceIds,
                [definition],
                $"Added derived column '{definition.Name}'");
        }

        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("Column Added")
            .WithContent($"Successfully added the derived column '{definition.Name}'.")
            .Queue();

        // The columns the editor is showing are read again so the new column appears, and the table's own
        // grid is read again so the values worked out for it are shown.
        viewModel.DataSource.Invalidate();

        if (context.TryGetTableDocument(out var document))
        {
            document.DataSource.Invalidate();
        }

        if (added.Count > 0)
        {
            // The new column is put under the cursor so it can be renamed or worked on without hunting for it.
            viewModel.SelectColumnAsync(added[0].Id).FireAndForget();
        }
    }

    /// <summary>
    ///     Asks for the operation, the columns it reads and the way it is set up.
    /// </summary>
    /// <returns>The settings that were asked for, or <see langword="null" /> when the dialog was dismissed.</returns>
    private async Task<ComputedColumnSettings?> AskForSettingsAsync(IReadOnlyList<ColumnEntity> columns)
    {
        var nameBox = new TextBox { PlaceholderText = "New column name" };

        var operationList = CreateChoices(Operations.Select(operation => operation.Label));
        operationList.SelectedIndex = 0;

        var sourceList = CreateChoices(columns.Select(column => column.Name), maxHeight: 200);
        sourceList.SelectionMode = SelectionMode.Multiple;

        // Each operation is set up its own way, so only the settings that belong to the chosen operation are
        // shown and the rest are left out of the way.
        var separatorBox = new TextBox { Text = ", ", PlaceholderText = "Separator" };
        var skipBlankBox = new CheckBox { Content = "Leave out the cells that are empty", IsChecked = true };

        var splitSeparatorBox = new TextBox { Text = ",", PlaceholderText = "Separator" };
        var partBox = new TextBox { Text = "0", PlaceholderText = "Part number" };

        var startBox = new TextBox { Text = "0", PlaceholderText = "Start" };
        var lengthBox = new TextBox { Text = "-1", PlaceholderText = "Length" };

        var patternBox = new TextBox { PlaceholderText = "Pattern" };
        var groupBox = new TextBox { Text = "1", PlaceholderText = "Capture number" };

        var operatorList = CreateChoices(MathOperatorLabels, columns: 4);
        operatorList.SelectedIndex = 0;

        var placesBox = new TextBox { Text = "2", PlaceholderText = "Decimal places" };

        var datePartList = CreateChoices(DatePartLabels, columns: 4);
        datePartList.SelectedIndex = 0;

        var concatenatePanel = Panel(
            Labelled("Separator", separatorBox),
            skipBlankBox);

        var splitPanel = Panel(
            Labelled("Separator", splitSeparatorBox),
            Labelled("Part number", partBox));

        var charactersPanel = Panel(
            Labelled("Start", startBox),
            Labelled("Length", lengthBox));

        var patternPanel = Panel(
            Labelled("Pattern", patternBox),
            Labelled("Capture number", groupBox));

        var calculatePanel = Panel(
            Labelled("Operator", operatorList),
            Labelled("Decimal places", placesBox));

        var datePartPanel = Panel(
            Labelled("Date part", datePartList));

        var parameterPanels = new (ComputedOperation Operation, Control Panel)[]
        {
            (ComputedOperation.Concatenate, concatenatePanel),
            (ComputedOperation.Split, splitPanel),
            (ComputedOperation.ExtractCharacters, charactersPanel),
            (ComputedOperation.ExtractPattern, patternPanel),
            (ComputedOperation.Calculate, calculatePanel),
            (ComputedOperation.DatePart, datePartPanel),
        };

        void ShowSettingsFor(ComputedOperation operation)
        {
            foreach (var (owned, panel) in parameterPanels)
            {
                panel.IsVisible = owned == operation;
            }
        }

        operationList.SelectionChanged += (_, _) =>
        {
            var index = Math.Clamp(operationList.SelectedIndex, 0, Operations.Length - 1);

            ShowSettingsFor(Operations[index].Operation);
        };

        ShowSettingsFor(Operations[0].Operation);

        var confirmed = await dialogManager.CreateDialog()
            .WithTitle("Add Derived Column")
            .WithContentList(
            [
                Labelled("Column name", nameBox),
                Labelled("Operation", operationList),
                Labelled("Source columns", sourceList),
                concatenatePanel,
                splitPanel,
                charactersPanel,
                patternPanel,
                calculatePanel,
                datePartPanel,
            ])
            .Dismiss().ByClickingBackground()
            .WithYesNoResult("Add", "Cancel")
            .TryShowAsync();

        if (!confirmed)
        {
            return null;
        }

        var operationIndex = Math.Clamp(operationList.SelectedIndex, 0, Operations.Length - 1);
        var operation = Operations[operationIndex].Operation;

        var selectedNames = ReadSelectedNames(sourceList);

        return new ComputedColumnSettings(
            operation,
            nameBox.Text?.Trim() ?? string.Empty,
            selectedNames,
            separatorBox.Text ?? string.Empty,
            skipBlankBox.IsChecked == true,
            splitSeparatorBox.Text ?? string.Empty,
            ReadInt(partBox, 0),
            ReadInt(startBox, 0),
            ReadInt(lengthBox, -1),
            patternBox.Text ?? string.Empty,
            ReadInt(groupBox, 1),
            (MathOperator)Math.Clamp(operatorList.SelectedIndex, 0, MathOperatorLabels.Length - 1),
            Math.Clamp(ReadInt(placesBox, 2), 0, 12),
            (DatePartKind)Math.Clamp(datePartList.SelectedIndex, 0, DatePartLabels.Length - 1));
    }

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
    ///     Reads a number out of a box, falling back to a sensible starting point when what is typed there
    ///     does not read as one.
    /// </summary>
    private static int ReadInt(TextBox box, int fallback)
    {
        return int.TryParse(box.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : fallback;
    }

    /// <summary>
    ///     Builds the picker a choice is made with.
    /// </summary>
    private static ListBox CreateChoices(IEnumerable<string> choices, double maxHeight = double.PositiveInfinity,
        int columns = 2)
    {
        return new ListBox
        {
            ItemsSource = choices.ToList(),
            SelectionMode = SelectionMode.AlwaysSelected | SelectionMode.Single,
            MaxHeight = maxHeight,
            ItemsPanel = new FuncTemplate<Panel?>(() => new UniformGrid
            {
                Columns = columns,
                RowSpacing = 10,
                ColumnSpacing = 10,
            }),
        };
    }

    /// <summary>
    ///     Puts a caption above a control so what it is asking for is not left to be guessed.
    /// </summary>
    private static StackPanel Labelled(string caption, Control control)
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

    private static StackPanel Panel(params Control[] children)
    {
        var panel = new StackPanel { Spacing = 10 };

        foreach (var child in children)
        {
            panel.Children.Add(child);
        }

        return panel;
    }

    /// <summary>
    ///     Which arithmetic, or which part of a moment, a derived column is worked out with.
    /// </summary>
    private enum ComputedOperation
    {
        Concatenate,
        Split,
        ExtractCharacters,
        ExtractPattern,
        Calculate,
        DatePart,
    }

    /// <summary>
    ///     What was asked for in the dialog, in a form that can be turned into the column to add.
    /// </summary>
    private sealed record ComputedColumnSettings(
        ComputedOperation Operation,
        string Name,
        IReadOnlyList<string> SourceNames,
        string Separator,
        bool SkipBlank,
        string SplitSeparator,
        int PartIndex,
        int Start,
        int Length,
        string Pattern,
        int Group,
        MathOperator MathOperator,
        int DecimalPlaces,
        DatePartKind DatePartKind)
    {
        /// <summary>
        ///     The columns whose values are read, in the order they sit in the table.
        /// </summary>
        public IReadOnlyList<int> ResolveSourceColumnIds(IReadOnlyList<ColumnEntity> columns)
        {
            var wanted = new HashSet<string>(SourceNames, StringComparer.Ordinal);

            return [.. columns.Where(column => wanted.Contains(column.Name)).Select(column => column.Id)];
        }

        /// <summary>
        ///     Builds the column to add, or <see langword="null" /> when what was asked for cannot be built.
        /// </summary>
        public ComputedColumnDefinition? BuildDefinition(IReadOnlyList<ColumnEntity> columns)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return null;
            }

            var sourceCount = ResolveSourceColumnIds(columns).Count;

            if (sourceCount == 0)
            {
                return null;
            }

            // A value is only ever the first of the row's source values unless the operation is one that
            // reads the whole row, so the meanings of the settings are kept next to the operation itself.
            return Operation switch
            {
                ComputedOperation.Concatenate => new ComputedColumnDefinition(
                    Name,
                    ColumnDataType.Text,
                    values => ComputedValueOperations.Concatenate(values, Separator, SkipBlank)),

                ComputedOperation.Split => new ComputedColumnDefinition(
                    Name,
                    ColumnDataType.Text,
                    values => ComputedValueOperations.SplitPart(values[0], SplitSeparator, PartIndex)),

                ComputedOperation.ExtractCharacters => new ComputedColumnDefinition(
                    Name,
                    ColumnDataType.Text,
                    values => ComputedValueOperations.ExtractSubstring(values[0], Start, Length)),

                ComputedOperation.ExtractPattern => string.IsNullOrEmpty(Pattern)
                    ? null
                    : new ComputedColumnDefinition(
                        Name,
                        ColumnDataType.Text,
                        values => ComputedValueOperations.ExtractMatch(values[0], Pattern, Group)),

                ComputedOperation.Calculate => new ComputedColumnDefinition(
                    Name,
                    DecimalPlaces > 0 ? ColumnDataType.Decimal : ColumnDataType.Integer,
                    values => ComputedValueOperations.Calculate(values, MathOperator, DecimalPlaces)),

                ComputedOperation.DatePart => new ComputedColumnDefinition(
                    Name,
                    ComputedValueOperations.IsNumericPart(DatePartKind)
                        ? ColumnDataType.Integer
                        : ColumnDataType.Text,
                    values => ComputedValueOperations.DatePart(values[0], DatePartKind)),

                _ => null,
            };
        }
    }
}
