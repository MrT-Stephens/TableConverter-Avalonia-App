using System;
using System.Linq;
using System.Threading.Tasks;
using ModelFlow.DataVirtualization.DataManagement;
using SukiUI.Dialogs;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;
using TableConverter.Extensions;
using TableConverter.Utilities.Database.Models.TableStore;
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
        "EditIcon",
        "",
        0,
        ["Ctrl+Shift+E"]);

    public bool CanExecute(object? parameter, ICommandContext context)
    {
        return context.TryGetSelectedItems<DataItem<ColumnEntity>>(out var selectedItems)
            && selectedItems.Count == 1;
    }

    public async Task Execute(object? parameter, ICommandContext context)
    {
        if (!context.TryGetSelectedItem<DataItem<ColumnEntity>>(out var dataItem))
        {
            context.Cancel("No column selected.");
            return;
        }
        
        var column = dataItem.Item;

        string[] ignoredProperties =
        [
            nameof(column.Id),
            nameof(column.Cells),
        ];

        _ = await dialogManager.CreateDialog()
            .WithTitle("Edit Column")
            .WithForm(column, ignoredProperties)
            .Dismiss().ByClickingBackground()
            .WithOkResult("Ok")
            .TryShowAsync();
    }
}