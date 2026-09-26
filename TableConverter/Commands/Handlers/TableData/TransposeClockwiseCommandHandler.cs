using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;
using TableConverter.Utilities.Database;
using TableConverter.Utilities.Database.Interfaces;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string TransposeClockwise = "TableData.TransposeClockwise";
}

/// <summary>
/// Turns the table a quarter turn to the right.
/// </summary>
/// <remarks>
/// The left of the table comes up to the top, so the headings of the result are the column that was on
/// the left, read from the bottom up.
/// </remarks>
public class TransposeClockwiseCommandHandler(
    ISukiDialogManager dialogManager,
    ISukiToastManager toastManager,
    ITableStoreDbContextFactory databaseContextFactory)
    : TableRotationCommandHandler(dialogManager, toastManager, databaseContextFactory)
{
    protected override TableRotation Rotation => TableRotation.Clockwise;

    protected override string DirectionName => "Clockwise";

    public override ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.TransposeClockwise,
        "Transpose Clockwise",
        "Turn the table a quarter turn clockwise, so its rows become columns and its columns become rows.",
        "RotateClockwiseIcon",
        "Tools",
        2,
        ["Ctrl+Shift+X"],
        canSetLoadingState: true,
        canSetLoadingOnWorkspace: true);
}

