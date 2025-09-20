using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using TableConverter.Views.Controls.OverlayShared;

namespace TableConverter.Views.Controls;

public class BaseView : ContentControl
{
    public const string PART_DialogHost = "PART_DialogHost";
    
    public static readonly StyledProperty<bool> IsTitleBarVisibleProperty = 
        AvaloniaProperty.Register<BaseView, bool>(nameof(IsTitleBarVisible), true);
    
    public static readonly StyledProperty<object?> TitleBarContentProperty =
        AvaloniaProperty.Register<BaseView, object?>(nameof(TitleBarContent));
    
    public static readonly StyledProperty<object?> LeftContentProperty = 
        AvaloniaProperty.Register<BaseView, object?>(nameof(LeftContent));
    
    public static readonly StyledProperty<object?> RightContentProperty =
        AvaloniaProperty.Register<BaseView, object?>(nameof(RightContent));
    
    public static readonly StyledProperty<Thickness> TitleBarMarginProperty =
        AvaloniaProperty.Register<BaseView, Thickness>(nameof(TitleBarMargin));
    
    public bool IsTitleBarVisible
    {
        get => GetValue(IsTitleBarVisibleProperty);
        set => SetValue(IsTitleBarVisibleProperty, value);
    }
    
    public object? LeftContent
    {
        get => GetValue(LeftContentProperty);
        set => SetValue(LeftContentProperty, value);
    }
    
    public object? RightContent
    {
        get => GetValue(RightContentProperty);
        set => SetValue(RightContentProperty, value);
    }
    
    public object? TitleBarContent
    {
        get => GetValue(TitleBarContentProperty);
        set => SetValue(TitleBarContentProperty, value);
    }
    
    public Thickness TitleBarMargin
    {
        get => GetValue(TitleBarMarginProperty);
        set => SetValue(TitleBarMarginProperty, value);
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var host = e.NameScope.Find<OverlayDialogHost>(PART_DialogHost);
        if (host is not null) LogicalChildren.Add(host);
    }
}