using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Extensions;
using TableConverter.Commands.Interfaces;
using TableConverter.Utilities.Database.History;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string ClearHistory = "TableData.ClearHistory";
}

/// <summary>
/// Throws away the record of the changes made to a table.
/// </summary>
/// <remarks>
/// Only the steps are discarded - the table itself is left exactly as it is - so clearing the history
/// costs nothing but the ability to take those steps back. That ability cannot itself be restored, which
/// is why the command asks before it acts.
/// </remarks>
public class ClearTableHistoryCommandHandler(
    ISukiDialogManager dialogManager,
    ISukiToastManager toastManager,
    ITableHistory history)
    : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.ClearHistory,
        "Clear History",
        "Throw away the record of the changes made to the table.",
        "TrashIcon",
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

        if (!await dialogManager.CreateDialog()
                .WithTitle("Clear the history?")
                .WithContent("This will throw away the record of the changes made to this table. The table "
                             + "itself is left as it is, but the changes can no longer be taken back.")
                .WithYesNoResult("Clear", "Cancel")
                .TryShowAsync())
        {
            return;
        }

        await history.ClearAsync(document.Path);

        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("History Cleared")
            .WithContent("The changes made to this table can no longer be taken back.")
            .Queue();
    }
}

