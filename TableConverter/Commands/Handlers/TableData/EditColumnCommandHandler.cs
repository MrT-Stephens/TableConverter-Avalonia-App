using System;
using System.Linq;
using System.Threading.Tasks;
using SukiUI.Dialogs;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;
using TableConverter.Extensions;
using TableConverter.Utilities.Extensions;
using TableConverter.ViewModels.Tools;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string EditColumn = "TableData.EditColumn";
}

public class EditColumnCommandHandler(ISukiDialogManager dialogManager) : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.EditColumn,
        "Edit Column",
        "Edit the selected column's properties.",
        "DataAddIcon",
        "",
        0,
        ["Ctrl+Shift+E"]);

    public bool CanExecute(object? parameter, ICommandContext context)
    {
        return context.TryGetSelectedItem<TableColumnsEditorViewModel>(out var columnsEditorViewModel)
               && columnsEditorViewModel.SelectedColumns is { Count: 1 };
    }

    public async Task Execute(object? parameter, ICommandContext context)
    {
        if (!context.TryGetSelectedItem<TableColumnsEditorViewModel>(out var columnsEditorViewModel))
        {
            throw new InvalidOperationException("No TableColumnsEditorViewModel selected.");
        }

        var selectedColumn = columnsEditorViewModel.SelectedColumns.FirstOrDefault()?.Item;

        if (selectedColumn is null)
        {
            context.Cancel("No column selected.");
            return;
        }

        var column = await columnsEditorViewModel.DataSource.GetItemAsync(x => x.Id == selectedColumn.Id);
        
        if (column is null)
        {
            context.Cancel("Selected column not found in data source.");
            return;
        }

        string[] ignoredProperties =
        [
            nameof(column.Id),
            nameof(column.Cells)
        ];

        var result = await dialogManager.CreateDialog()
            .WithTitle("Edit Column")
            .WithForm(column, ignoredProperties)
            .Dismiss().ByClickingBackground()
            .WithOkResult("Save")
            .TryShowAsync();

        if (result is false || column.CompareTo(selectedColumn, ignoredProperties))
        {
            return;
        }
        
        await columnsEditorViewModel.DataSource.UpdateAsync(column);
        columnsEditorViewModel.DataSource.Invalidate();
    }
}