using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using SukiUI.Toasts;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Extensions;
using TableConverter.Commands.Interfaces;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.Interfaces;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string TrimWhitespace = "TableData.TrimWhitespace";
}

/// <summary>
/// Trims the whitespace off both ends of every cell in the table.
/// </summary>
/// <remarks>
/// Whitespace around a value is what most often stops it matching a search or reading as the number it
/// looks like, and it is invisible, so it is worth being able to take it off the whole table at once.
/// </remarks>
public class TrimWhitespaceCommandHandler(
    ISukiToastManager toastManager,
    ITableStoreDbContextFactory databaseContextFactory)
    : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.TrimWhitespace,
        "Trim Whitespace",
        "Trim leading and trailing whitespace off every cell.",
        "EditIcon",
        "Tools",
        1,
        ["Ctrl+Shift+T"],
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

        // The column names are cleaned up alongside the cells, and a table whose values are already clean is
        // left alone rather than rewritten in full.
        var trimmed = await TableStoreMaintenance.Create(db).TrimAsync();

        if (trimmed == 0)
        {
            toastManager.CreateSimpleInfoToast()
                .OfType(NotificationType.Information)
                .WithTitle("Nothing To Trim")
                .WithContent("No cell in the table starts or ends with whitespace.")
                .Queue();

            return;
        }

        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("Whitespace Trimmed")
            .WithContent($"Successfully trimmed {trimmed} cell(s).")
            .Queue();

        document.DataSource.Invalidate();
    }
}
