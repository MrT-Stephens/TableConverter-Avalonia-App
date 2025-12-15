using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace TableConverter.Views.Controls;

public class DockedToolPanel : TemplatedControl
{
    public static readonly StyledProperty<object?> MainContentProperty =
        AvaloniaProperty.Register<DockedToolPanel, object?>(
            nameof(MainContent));

    public static readonly StyledProperty<object?> ToolContentProperty =
        AvaloniaProperty.Register<DockedToolPanel, object?>(
            nameof(ToolContent));

    public static readonly StyledProperty<Dock> DockPositionProperty =
        AvaloniaProperty.Register<DockedToolPanel, Dock>(
            nameof(DockPosition), Dock.Left);

    protected override Type StyleKeyOverride => typeof(DockedToolPanel);

    public object? MainContent
    {
        get => GetValue(MainContentProperty);
        set => SetValue(MainContentProperty, value);
    }

    public object? ToolContent
    {
        get => GetValue(ToolContentProperty);
        set => SetValue(ToolContentProperty, value);
    }

    public Dock DockPosition
    {
        get => GetValue(DockPositionProperty);
        set => SetValue(DockPositionProperty, value);
    }
}