using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Threading;
using System.Threading.Tasks;
using TableConverter.Helpers;
using TableConverter.Views.Controls.Dialog;
using TableConverter.Views.Controls.OverlayShared.Events;
using TableConverter.Views.Controls.OverlayShared.Interfaces;

namespace TableConverter.Views.Controls.OverlayShared;

public abstract class OverlayFeedbackElement : ContentControl
{
    public static readonly StyledProperty<bool> IsClosedProperty = 
        AvaloniaProperty.Register<OverlayFeedbackElement, bool>(nameof(IsClosed), true);

    public static readonly RoutedEvent<OverlayResultEventArgs> ClosedEvent = 
        RoutedEvent.Register<DialogControlBase, OverlayResultEventArgs>(nameof(Closed), RoutingStrategies.Bubble);

    private bool _ResizeDragging;

    protected Panel? _ContainerPanel;
    private Rect _ResizeDragStartBounds;
    private Point _ResizeDragStartPoint;

    private WindowEdge? _WindowEdge;

    static OverlayFeedbackElement()
    {
        FocusableProperty.OverrideDefaultValue<OverlayFeedbackElement>(false);

        DataContextProperty.Changed.AddClassHandler<OverlayFeedbackElement, object?>((o, e) => o.OnDataContextChange(e));

        ClosedEvent.AddClassHandler<OverlayFeedbackElement>((o, e) => o.OnClosed(e));
    }

    public bool IsClosed
    {
        get => GetValue(IsClosedProperty);
        set => SetValue(IsClosedProperty, value);
    }

    private void OnClosed(OverlayResultEventArgs args)
    {
        SetCurrentValue(IsClosedProperty, true);
    }

    public event EventHandler<OverlayResultEventArgs> Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    private void OnDataContextChange(AvaloniaPropertyChangedEventArgs<object?> args)
    {
        if (args.OldValue.Value is IDialogContext oldContext) oldContext.RequestClose -= OnContextRequestClose;
        if (args.NewValue.Value is IDialogContext newContext) newContext.RequestClose += OnContextRequestClose;
    }

    protected virtual void OnElementClosing(object? sender, object? args)
    {
        RaiseEvent(new OverlayResultEventArgs(ClosedEvent, args));
    }

    private void OnContextRequestClose(object? sender, object? args)
    {
        RaiseEvent(new OverlayResultEventArgs(ClosedEvent, args));
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        Content = null;
    }

    internal bool IsShowAsync { get; set; }

    public Task<T?> ShowAsync<T>(CancellationToken? token = default)
    {
        IsShowAsync = true;
        var tcs = new TaskCompletionSource<T?>();
        token?.Register(() => { Dispatcher.UIThread.Invoke(Close); });

        void OnCloseHandler(object? sender, OverlayResultEventArgs? args)
        {
            IsShowAsync = false;
            if (args is not null)
                args.Handled = true;
            if (args?.Result is T result)
                tcs.SetResult(result);
            else
                tcs.SetResult(default);
            RemoveHandler(ClosedEvent, OnCloseHandler);
        }

        AddHandler(ClosedEvent, OnCloseHandler);
        return tcs.Task;
    }

    public abstract void Close();

    internal void BeginResizeDrag(WindowEdge windowEdge, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        _ResizeDragging = true;
        _ResizeDragStartPoint = e.GetPosition(this);
        _ResizeDragStartBounds = Bounds;
        _WindowEdge = windowEdge;
    }

    internal void BeginMoveDrag(PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        _ResizeDragging = true;
        _ResizeDragStartPoint = e.GetPosition(this);
        _ResizeDragStartBounds = Bounds;
        _WindowEdge = null;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _ContainerPanel = this.FindAncestorOfType<Panel>();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _ResizeDragging = false;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        _ResizeDragging = false;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!_ResizeDragging || _WindowEdge is null) return;
        var point = e.GetPosition(this);
        Vector diff = point - _ResizeDragStartPoint;
        var left = Canvas.GetLeft(this);
        var top = Canvas.GetTop(this);
        var width = _WindowEdge is WindowEdge.West or WindowEdge.NorthWest or WindowEdge.SouthWest
            ? Bounds.Width
            : _ResizeDragStartBounds.Width;
        var height = _WindowEdge is WindowEdge.North or WindowEdge.NorthEast or WindowEdge.NorthWest
            ? Bounds.Height
            : _ResizeDragStartBounds.Height;
        var newBounds = CalculateNewBounds(left, top, width, height, diff, _ContainerPanel?.Bounds, _WindowEdge.Value);
        Canvas.SetLeft(this, newBounds.Left);
        Canvas.SetTop(this, newBounds.Top);
        SetCurrentValue(WidthProperty, newBounds.Width);
        SetCurrentValue(HeightProperty, newBounds.Height);
        AnchorAndUpdatePositionInfo();
    }

    private Rect CalculateNewBounds(double left, double top, double width, double height, Vector diff,
        Rect? containerBounds,
        WindowEdge windowEdge)
    {
        diff = CoerceDelta(left, top, width, height, diff, containerBounds, windowEdge);
        switch (windowEdge)
        {
            case WindowEdge.North:
                top += diff.Y;
                height -= diff.Y;
                break;
            case WindowEdge.NorthEast:
                top += diff.Y;
                width += diff.X;
                height -= diff.Y;
                break;
            case WindowEdge.East:
                width += diff.X;
                break;
            case WindowEdge.SouthEast:
                width += diff.X;
                height += diff.Y;
                break;
            case WindowEdge.South:
                height += diff.Y;
                break;
            case WindowEdge.SouthWest:
                left += diff.X;
                width -= diff.X;
                height += diff.Y;
                break;
            case WindowEdge.West:
                left += diff.X;
                width -= diff.X;
                break;
            case WindowEdge.NorthWest:
                left += diff.X;
                top += diff.Y;
                width -= diff.X;
                height -= diff.Y;
                break;
        }

        return new Rect(left, top, width, height);
    }

    private Vector CoerceDelta(double left, double top, double width, double height, Vector diff, Rect? containerBounds,
        WindowEdge windowEdge)
    {
        if (containerBounds is null) return diff;
        var minX = windowEdge is WindowEdge.West or WindowEdge.NorthWest or WindowEdge.SouthWest
            ? -left
            : -width;
        var minY = windowEdge is WindowEdge.North or WindowEdge.NorthEast or WindowEdge.NorthWest
            ? -top
            : -height;
        var maxX = windowEdge is WindowEdge.West or WindowEdge.NorthWest or WindowEdge.SouthWest
            ? width - MinWidth
            : containerBounds.Value.Width - left - width;
        var maxY = windowEdge is WindowEdge.North or WindowEdge.NorthEast or WindowEdge.NorthWest
            ? height - MinWidth
            : containerBounds.Value.Height - top - height;
        return new Vector(MathHelpers.SafeClamp(diff.X, minX, maxX), MathHelpers.SafeClamp(diff.Y, minY, maxY));
    }

    protected internal abstract void AnchorAndUpdatePositionInfo();
}