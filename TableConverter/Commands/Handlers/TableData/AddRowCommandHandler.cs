using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using SukiUI.Toasts;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Extensions;
using TableConverter.Commands.Interfaces;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.History;
using TableConverter.Utilities.Database.Interfaces;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string AddRow = "TableData.AddRow";
}

/// <summary>
/// Appends an empty row to the selected table.
/// </summary>
/// <remarks>
/// The row is written to the store the moment it is added, like every other table operation, so the grid
/// never shows a row the store does not hold. Each of its cells starts from the default value its column
/// was given, which is what makes a column that is meant to hold something by default useful.
/// </remarks>
public class AddRowCommandHandler(
    ISukiToastManager toastManager,
    ITableStoreDbContextFactory databaseContextFactory,
    ITableHistory history)
    : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.AddRow,
        "Add Row",
        "Add a new row to the table.",
        "AddIcon",
        "Edit",
        1,
        ["Ctrl+Shift+N"],
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

        await using var db = await databaseContextFactory.CreateDbContextAsync(document.Path);

        // A row that did not exist has no identity until it has been written, so what the step did is
        // described by what the row holds once it is there rather than by what it held before.
        await using var edit = history.BeginEdit(document.Path, TableEditKind.RowsAdded, "Added a row");

        // The row and the cells of the columns it was added under are written together, so the store is
        // never left holding a row that is missing cells the grid would then bind against.
        var rowId = await TableStoreMaintenance.Create(db).AddRowAsync();

        await edit.CaptureAfterAsync(TableRegion.Rows([rowId]));

        await edit.CommitAsync();

        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("Row Added")
            .WithContent("A new row has been added to the table.")
            .Queue();

        document.DataSource.Invalidate();
    }
}