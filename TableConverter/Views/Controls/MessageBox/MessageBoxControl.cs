using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using TableConverter.Extenstions;
using TableConverter.Views.Controls.Dialog;
using TableConverter.Views.Controls.MessageBox.Enums;

namespace TableConverter.Views.Controls.MessageBox;

[TemplatePart(PART_NoButton, typeof(Button))]
[TemplatePart(PART_OKButton, typeof(Button))]
[TemplatePart(PART_CancelButton, typeof(Button))]
[TemplatePart(PART_YesButton, typeof(Button))]
public class MessageBoxControl : DialogControlBase
{
    public const string PART_YesButton = "PART_YesButton";
    public const string PART_NoButton = "PART_NoButton";
    public const string PART_OKButton = "PART_OKButton";
    public const string PART_CancelButton = "PART_CancelButton";

    public static readonly StyledProperty<MessageBoxIcon> MessageIconProperty =
        AvaloniaProperty.Register<MessageBoxControl, MessageBoxIcon>(nameof(MessageIcon));

    public static readonly StyledProperty<MessageBoxButton> ButtonsProperty =
        AvaloniaProperty.Register<MessageBoxControl, MessageBoxButton>(nameof(Buttons));

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<MessageBoxControl, string?>(nameof(Title));

    private Button? _CancelButton;
    private Button? _NoButton;
    private Button? _OkButton;
    private Button? _YesButton;

    static MessageBoxControl()
    {
        ButtonsProperty.Changed.AddClassHandler<MessageBoxControl>(
            (o, _) => { o.SetButtonVisibility(); });
    }

    public MessageBoxIcon MessageIcon
    {
        get => GetValue(MessageIconProperty);
        set => SetValue(MessageIconProperty, value);
    }

    public MessageBoxButton Buttons
    {
        get => GetValue(ButtonsProperty);
        set => SetValue(ButtonsProperty, value);
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Button.ClickEvent.RemoveHandler(DefaultButtonsClose, _OkButton, _CancelButton, _YesButton, _NoButton);
        _OkButton = e.NameScope.Find<Button>(PART_OKButton);
        _CancelButton = e.NameScope.Find<Button>(PART_CancelButton);
        _YesButton = e.NameScope.Find<Button>(PART_YesButton);
        _NoButton = e.NameScope.Find<Button>(PART_NoButton);
        Button.ClickEvent.AddHandler(DefaultButtonsClose, _OkButton, _CancelButton, _YesButton, _NoButton);
        SetButtonVisibility();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        
        var defaultButton = Buttons switch
        {
            MessageBoxButton.Ok => _OkButton,
            MessageBoxButton.OkCancel => _CancelButton,
            MessageBoxButton.YesNo => _YesButton,
            MessageBoxButton.YesNoCancel => _CancelButton,
            _ => null
        };
        
        defaultButton?.Focus();
    }

    private void DefaultButtonsClose(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) 
            return;
        
        var result = button switch
        {
            _ when button == _OkButton => MessageBoxResult.Ok,
            _ when button == _CancelButton => MessageBoxResult.Cancel,
            _ when button == _YesButton => MessageBoxResult.Yes,
            _ when button == _NoButton => MessageBoxResult.No,
            _ => MessageBoxResult.None
        };
        
        OnElementClosing(this, result);
    }

    private void SetButtonVisibility()
    {
        var closeButtonVisible = Buttons != MessageBoxButton.YesNo;
        IsVisibleProperty.SetValue(closeButtonVisible, _CloseButton);
        
        switch (Buttons)
        {
            case MessageBoxButton.Ok:
                IsVisibleProperty.SetValue(true, _OkButton);
                IsVisibleProperty.SetValue(false, _CancelButton, _YesButton, _NoButton);
                Button.IsDefaultProperty.SetValue(true, _OkButton);
                Button.IsDefaultProperty.SetValue(false, _CancelButton, _YesButton, _NoButton);
                break;
            case MessageBoxButton.OkCancel:
                IsVisibleProperty.SetValue(true, _OkButton, _CancelButton);
                IsVisibleProperty.SetValue(false, _YesButton, _NoButton);
                Button.IsDefaultProperty.SetValue(true, _OkButton);
                Button.IsDefaultProperty.SetValue(false, _CancelButton, _YesButton, _NoButton);
                break;
            case MessageBoxButton.YesNo:
                IsVisibleProperty.SetValue(false, _OkButton, _CancelButton);
                IsVisibleProperty.SetValue(true, _YesButton, _NoButton);
                break;
            case MessageBoxButton.YesNoCancel:
                IsVisibleProperty.SetValue(false, _OkButton);
                IsVisibleProperty.SetValue(true, _CancelButton, _YesButton, _NoButton);
                break;
        }
    }

    public override void Close()
    {
        var result = Buttons switch
        {
            MessageBoxButton.Ok => MessageBoxResult.Ok,
            MessageBoxButton.OkCancel => MessageBoxResult.Cancel,
            MessageBoxButton.YesNo => MessageBoxResult.No,
            MessageBoxButton.YesNoCancel => MessageBoxResult.Cancel,
            _ => MessageBoxResult.None
        };
        
        OnElementClosing(this, result);
    }
}