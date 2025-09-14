using System;
using TableConverter.Views.Controls.Dialog.Enums;
using TableConverter.Views.Controls.OverlayShared.Enums;

namespace TableConverter.Views.Controls.Dialog.Options;

public class OverlayDialogOptions
{
    internal static OverlayDialogOptions Default { get; } = new();

    /// <summary>
    ///     Whether the dialog should occupy the full screen. Default is false.
    /// </summary>
    public bool FullScreen { get; set; }

    /// <summary>
    ///     The horizontal anchor position of the dialog. Default is Center.
    /// </summary>
    public HorizontalPosition HorizontalAnchor { get; set; } = HorizontalPosition.Center;

    /// <summary>
    ///     The vertical anchor position of the dialog. Default is Center.
    /// </summary>
    public VerticalPosition VerticalAnchor { get; set; } = VerticalPosition.Center;

    /// <summary>
    ///     This attribute is only used when HorizontalAnchor is not Center
    /// </summary>
    public double? HorizontalOffset { get; set; } = null;

    /// <summary>
    ///     This attribute is only used when VerticalAnchor is not Center
    /// </summary>
    public double? VerticalOffset { get; set; } = null;

    /// <summary>
    ///     Only works for DefaultDialogControl
    /// </summary>
    public DialogMode Mode { get; set; } = DialogMode.None;

    /// <summary>
    ///     Only works for DefaultDialogControl
    /// </summary>
    public DialogButton Buttons { get; set; } = DialogButton.OkCancel;

    /// <summary>
    ///     Only works for DefaultDialogControl
    /// </summary>
    public string? Title { get; set; } = null;

    /// <summary>
    ///     Only works for CustomDialogControl
    /// </summary>
    public bool? IsCloseButtonVisible { get; set; } = true;

    [Obsolete] public bool ShowCloseButton { get; set; } = true;

    /// <summary>
    ///     Sets whether the dialog can be dismissed by clicking outside the dialog area. Default is true.
    /// </summary>
    public bool CanLightDismiss { get; set; } = true;

    /// <summary>
    ///     Can the Dialog be moved by dragging the title bar, Default is true
    /// </summary>
    public bool CanDragMove { get; set; } = true;

    /// <summary>
    ///     The hash code of the top level dialog host. This is used to identify the dialog host if there are multiple dialog
    ///     hosts with the same id. If this is not provided, the dialog will be added to the first dialog host with the same
    ///     id.
    /// </summary>
    public int? TopLevelHashCode { get; set; }

    /// <summary>
    ///     Can the Dialog be resized, Default is false
    /// </summary>
    public bool CanResize { get; set; }

    /// <summary>
    ///     Extra Style Class for Dialog
    /// </summary>
    public string? StyleClass { get; set; }
}
