using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.LogicalTree;
using TableConverter.Views.Controls.OverlayShared;
using TableConverter.Views.Controls.Resizer.Enums;

namespace TableConverter.Views.Controls.Resizer;

public class DialogResizerThumb : Thumb
{
    private OverlayFeedbackElement? _Dialog;
    
    public static readonly StyledProperty<ResizeDirection> ResizeDirectionProperty = 
        AvaloniaProperty.Register<DialogResizerThumb, ResizeDirection>(nameof(ResizeDirection));

    public ResizeDirection ResizeDirection
    {
        get => GetValue(ResizeDirectionProperty);
        set => SetValue(ResizeDirectionProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _Dialog = this.FindLogicalAncestorOfType<OverlayFeedbackElement>();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        if (_Dialog is null) 
            return;
        
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) 
            return;
        
        var windowEdge = ResizeDirection switch
        {
            ResizeDirection.Top => WindowEdge.North,
            ResizeDirection.TopRight => WindowEdge.NorthEast,
            ResizeDirection.Right => WindowEdge.East,
            ResizeDirection.BottomRight => WindowEdge.SouthEast,
            ResizeDirection.Bottom => WindowEdge.South,
            ResizeDirection.BottomLeft => WindowEdge.SouthWest,
            ResizeDirection.Left => WindowEdge.West,
            ResizeDirection.TopLeft => WindowEdge.NorthWest,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        _Dialog.BeginResizeDrag(windowEdge, e);
    }
}