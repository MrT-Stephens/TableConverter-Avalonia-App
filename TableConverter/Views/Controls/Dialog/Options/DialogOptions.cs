using Avalonia;
using Avalonia.Controls;
using TableConverter.Views.Controls.OverlayShared.Enums;

namespace TableConverter.Views.Controls.Dialog.Options;

public class DialogOptions
{
    internal static DialogOptions Default { get; } = new DialogOptions();

    /// <summary>
    /// The Startup Location of DialogWindow. Default is <see cref="WindowStartupLocation.CenterOwner"/>
    /// </summary>
    public WindowStartupLocation StartupLocation { get; set; } = WindowStartupLocation.CenterOwner;

    /// <summary>
    /// The Position of DialogWindow startup location if <see cref="StartupLocation"/> is <see cref="WindowStartupLocation.Manual"/>
    /// </summary>
    public PixelPoint? Position { get; set; }

    /// <summary>
    /// Title of DialogWindow, Default is null
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// DialogWindow's Mode, Default is <see cref="DialogMode.None"/>
    /// </summary>
    public DialogMode Mode { get; set; } = DialogMode.None;

    /// <summary>
    /// Buttons of DialogWindow, Default is <see cref="DialogButton.OkCancel"/>
    /// </summary>
    public DialogButton Button { get; set; } = DialogButton.OkCancel;

    /// <summary>
    /// Is Close Button Visible on DialogWindow, Default is true
    /// </summary>
    public bool? IsCloseButtonVisible { get; set; } = true;

    public bool ShowInTaskBar { get; set; } = true;

    /// <summary>
    /// Can the DialogWindow be moved by dragging the title bar, Default is true
    /// </summary>
    public bool CanDragMove { get; set; } = true;

    /// <summary>
    /// Can the DialogWindow be resized, Default is false
    /// </summary>
    public bool CanResize { get; set; }

    /// <summary>
    /// Extra Style Class for DialogWindow
    /// </summary>
    public string? StyleClass { get; set; }
}
