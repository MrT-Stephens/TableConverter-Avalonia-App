using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using ModelFlow.DataVirtualization.DataManagement;
using SukiUI.Toasts;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Extensions;
using TableConverter.Commands.Interfaces;
using TableConverter.Extensions;
using TableConverter.Utilities.Database.Models.TableStore;
using TableConverter.ViewModels.Tools;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string DuplicateColumn = "TableData.DuplicateColumn";
}

/// <summary>
/// Adds a column beside the selected one, holding the same kind of value and what it holds.
/// </summary>
public class DuplicateColumnCommandHandler(ISukiToastManager toastManager)
    : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.DuplicateColumn,
        "Duplicate Column",
        "Add a copy of the selected column beside it, holding what it holds.",
        "DuplicateIcon",
        canSetLoadingState: true,
        canSetLoadingOnWorkspace: true);

    public bool CanExecute(object? parameter, ICommandContext context)
    {
        return context.TryGetSelectedItem<TableColumnsEditorViewModel>(out _)
               && context.TryGetSelectedItem<DataItem<ColumnEntity>>(out _);
    }

    public async Task Execute(object? parameter, ICommandContext context)
    {
        if (!context.TryGetSelectedItem<TableColumnsEditorViewModel>(out var viewModel))
        {
            context.Cancel("No table columns editor found.");
            return;
        }

        if (!context.TryGetSelectedItem<DataItem<ColumnEntity>>(out var column))
        {
            context.Cancel("No column selected.");
            return;
        }

        var copy = await viewModel.DataSource.DuplicateAsync(column.Item);

        if (copy is null)
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Error)
                .WithTitle("Column Not Duplicated")
                .WithContent($"'{column.Item.Name}' could not be copied.")
                .Queue();

            return;
        }

        // The columns editor shows the columns in the order the store keeps them, so it is read again and
        // the copy is put under the cursor ready to be renamed. The table's own grid is rebuilt as well,
        // because a copy makes room for itself by moving the columns that follow it along.
        viewModel.DataSource.Invalidate();
        viewModel.SelectColumnAsync(copy.Id).FireAndForget();

        if (context.TryGetTableDocument(out var document))
        {
            await document.InvalidateDataAsync();
        }

        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("Column Duplicated")
            .WithContent($"'{column.Item.Name}' was copied to '{copy.Name}'.")
            .Queue();
    }
}

