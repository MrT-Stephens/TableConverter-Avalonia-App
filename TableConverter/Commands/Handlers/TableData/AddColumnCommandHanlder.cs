using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;
using TableConverter.Extensions;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.ViewModels.Tools;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string AddColumn = "TableData.AddColumn";
}

public class AddColumnCommandHandler(
    ISukiToastManager toastManager,
    ISukiDialogManager dialogManager) 
    : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.AddColumn,
        "Add Column",
        "Add a new column to the table.",
        "PlusIcon",
        "",
        0,
        ["Ctrl+Shift+A"],
        canSetLoadingState: true,
        canSetLoadingOnWorkspace: true);

    public bool CanExecute(object? parameter, ICommandContext context)
    {
        return context.TryGetSelectedItem<TableColumnsEditorViewModel>(out _);
    }

    public async Task Execute(object? parameter, ICommandContext context)
    {
        if (!context.TryGetSelectedItem<TableColumnsEditorViewModel>(out var viewModel))
        {
            context.Cancel("No table columns editor found.");
            return;
        }
        
        var column = new ColumnEntity
        {
            Name = string.Empty
        };

        var (success, _, _) = await viewModel.DataSource.CreateAsync(column);

        if (!success)
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Error)
                .WithTitle("Error Adding Column")
                .WithContent($"Failed to add a new column.")
                .Queue();
        }
        else
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Success)
                .WithTitle("Column Added")
                .WithContent($"Successfully added a new column.")
                .Queue();
        }
        
        viewModel.DataSource.Invalidate();
    }
}