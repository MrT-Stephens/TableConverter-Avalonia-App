using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using TableConverter.Extenstions;
using TableConverter.Views.Controls.OverlayShared;
using TableConverter.Views.Controls.OverlayShared.Enums;
using TableConverter.Views.Controls.OverlayShared.Events;
using TableConverter.Views.Controls.OverlayShared.Interfaces;

namespace TableConverter.Views.Controls.Drawer;

[TemplatePart(PART_CloseButton, typeof(Button))]
public abstract class DrawerControlBase : OverlayFeedbackElement
{
    public const string PART_CloseButton = "PART_CloseButton";

    protected internal Button? _CloseButton;

    public static readonly StyledProperty<Position> PositionProperty =
        AvaloniaProperty.Register<DrawerControlBase, Position>(nameof(Position), defaultValue: Position.Right);

    public static readonly StyledProperty<bool> CanResizeProperty = 
        AvaloniaProperty.Register<DrawerControlBase, bool>(nameof(CanResize));

    public bool CanResize
    {
        get => GetValue(CanResizeProperty);
        set => SetValue(CanResizeProperty, value);
    }

    public Position Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public static readonly StyledProperty<bool> IsOpenProperty = 
        AvaloniaProperty.Register<DrawerControlBase, bool>(nameof(IsOpen));

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    internal bool? IsCloseButtonVisible { get; set; }

    protected internal bool CanLightDismiss { get; set; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Button.ClickEvent.RemoveHandler(OnCloseButtonClick, _CloseButton);
        _CloseButton = e.NameScope.Find<Button>(PART_CloseButton);
        Button.ClickEvent.AddHandler(OnCloseButtonClick, _CloseButton);
    }

    private void OnCloseButtonClick(object? sender, RoutedEventArgs e) => Close();

    public override void Close()
    {
        if (DataContext is IDialogContext context)
        {
            context.Close();
        }
        else
        {
            RaiseEvent(new OverlayResultEventArgs(ClosedEvent, null));
        }
    }
}
