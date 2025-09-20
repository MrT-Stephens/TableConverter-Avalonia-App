using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using TableConverter.Extenstions;
using TableConverter.Views.Controls.Resizer.Enums;

namespace TableConverter.Views.Controls.Resizer;

public class DialogResizer : TemplatedControl
{
    public const string PART_Top = "PART_Top";
    public const string PART_Bottom = "PART_Bottom";
    public const string PART_Left = "PART_Left";
    public const string PART_Right = "PART_Right";
    public const string PART_TopLeft = "PART_TopLeft";
    public const string PART_TopRight = "PART_TopRight";
    public const string PART_BottomLeft = "PART_BottomLeft";
    public const string PART_BottomRight = "PART_BottomRight";
    
    private Thumb? _Top;
    private Thumb? _Bottom;
    private Thumb? _Left;
    private Thumb? _Right;
    private Thumb? _TopLeft;
    private Thumb? _TopRight;
    private Thumb? _BottomLeft;
    private Thumb? _BottomRight;
    
    public static readonly StyledProperty<ResizeDirection> ResizeDirectionProperty = 
        AvaloniaProperty.Register<DialogResizer, ResizeDirection>(nameof(ResizeDirection), ResizeDirection.All);

    /// <summary>
    /// Defines what direction the dialog is allowed to be resized.
    /// </summary>
    public ResizeDirection ResizeDirection
    {
        get => GetValue(ResizeDirectionProperty);
        set => SetValue(ResizeDirectionProperty, value);
    }
    
    static DialogResizer()
    {
        ResizeDirectionProperty.Changed
            .AddClassHandler<DialogResizer, ResizeDirection>(
                (resizer, e) => resizer.OnResizeDirectionChanged(e));
    }

    private void OnResizeDirectionChanged(AvaloniaPropertyChangedEventArgs<ResizeDirection> args)
    {
        UpdateThumbVisibility(args.NewValue.Value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _Top = e.NameScope.Find<Thumb>(PART_Top);
        _Bottom = e.NameScope.Find<Thumb>(PART_Bottom);
        _Left = e.NameScope.Find<Thumb>(PART_Left);
        _Right = e.NameScope.Find<Thumb>(PART_Right);
        _TopLeft = e.NameScope.Find<Thumb>(PART_TopLeft);
        _TopRight = e.NameScope.Find<Thumb>(PART_TopRight);
        _BottomLeft = e.NameScope.Find<Thumb>(PART_BottomLeft);
        _BottomRight = e.NameScope.Find<Thumb>(PART_BottomRight);
        UpdateThumbVisibility(ResizeDirection);
    }

    private void UpdateThumbVisibility(ResizeDirection direction)
    {
        IsVisibleProperty.SetValue(direction.HasFlag(ResizeDirection.Top), _Top);
        IsVisibleProperty.SetValue(direction.HasFlag(ResizeDirection.Bottom), _Bottom);
        IsVisibleProperty.SetValue(direction.HasFlag(ResizeDirection.Left), _Left);
        IsVisibleProperty.SetValue(direction.HasFlag(ResizeDirection.Right), _Right);
        IsVisibleProperty.SetValue(direction.HasFlag(ResizeDirection.TopLeft), _TopLeft);
        IsVisibleProperty.SetValue(direction.HasFlag(ResizeDirection.TopRight), _TopRight);
        IsVisibleProperty.SetValue(direction.HasFlag(ResizeDirection.BottomLeft), _BottomLeft);
        IsVisibleProperty.SetValue(direction.HasFlag(ResizeDirection.BottomRight), _BottomRight);
    }
}