using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.Interfaces;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string TransposeCounterClockwise = "TableData.TransposeCounterClockwise";
}

/// <summary>
/// Turns the table a quarter turn to the left.
/// </summary>
/// <remarks>
/// The right of the table comes up to the top, so the headings of the result are the column that was on
/// the right, read from the top down.
/// </remarks>
public class TransposeCounterClockwiseCommandHandler(
    ISukiDialogManager dialogManager,
    ISukiToastManager toastManager,
    ITableStoreDbContextFactory databaseContextFactory)
    : TableRotationCommandHandler(dialogManager, toastManager, databaseContextFactory)
{
    protected override TableRotation Rotation => TableRotation.CounterClockwise;

    protected override string DirectionName => "Counter Clockwise";

    public override ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.TransposeCounterClockwise,
        "Transpose Counter Clockwise",
        "Turn the table a quarter turn anticlockwise, so its rows become columns and its columns become rows.",
        "RotateCounterClockwiseIcon",
        "Tools",
        3,
        ["Ctrl+Shift+W"],
        canSetLoadingState: true,
        canSetLoadingOnWorkspace: true);
}

