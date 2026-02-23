using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using ModelFlow.DataVirtualization.DataManagement;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.ViewModels.Tools;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string DeleteColumn = "TableData.DeleteColumn";
}

public class DeleteColumnCommandHandler(
    ISukiDialogManager dialogManager,
    ISukiToastManager toastManager) 
    : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.DeleteColumn,
        "Delete Column",
        "Delete the selected column(s).",
        "DeleteIcon",
        "",
        0,
        ["Ctrl+Shift+D"],
        canSetLoadingState: true,
        canSetLoadingOnWorkspace: true);

    public bool CanExecute(object? parameter, ICommandContext context)
    {
        return context.TryGetSelectedItems<DataItem<ColumnEntity>>(out var selectedItems)
               && selectedItems.Count > 0;
    }

    public async Task Execute(object? parameter, ICommandContext context)
    {
        if (!context.TryGetSelectedItem<TableColumnsEditorViewModel>(out var viewModel))
        {
            context.Cancel("No table columns editor found.");
            return;
        }
        
        if (!context.TryGetSelectedItems<DataItem<ColumnEntity>>(out var columns))
        {
            context.Cancel("No column selected.");
            return;
        }
        
        var result = await dialogManager.CreateDialog()
            .WithTitle("Are you sure?")
            .WithContent($"You are about to delete {columns.Count} column(s). Are you sure you want to proceed?")
            .Dismiss().ByClickingBackground()
            .WithYesNoResult("Yes", "No")
            .TryShowAsync();
        
        if (!result)
        {
            return;
        }
        
        var deletedCount = 0;
            
        foreach (var column in columns)
        {
            deletedCount += await viewModel.DataSource.DeleteAsync(column) ? 1 : 0;
        }

        if (deletedCount < columns.Count)
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Error)
                .WithTitle("Error Deleting Columns")
                .WithContent($"Failed to delete {columns.Count - deletedCount} out of {columns.Count} columns.")
                .Queue();
        }
        else
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Success)
                .WithTitle("Columns Deleted")
                .WithContent($"Successfully deleted {columns.Count} columns.")
                .Queue();
        }
        
        viewModel.DataSource.Invalidate();
    }
}