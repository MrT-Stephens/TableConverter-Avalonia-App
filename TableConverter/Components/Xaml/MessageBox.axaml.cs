using Avalonia;
using Avalonia.Controls;
using System.Collections.ObjectModel;

namespace TableConverter.Components.Xaml;

public partial class MessageBox : UserControl
{
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<MessageBox, object?>(nameof(Icon));

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<MessageBox, string>(nameof(Title));

    public static readonly StyledProperty<ObservableCollection<Control>> ActionButtonsProperty =
        AvaloniaProperty.Register<MessageBox, ObservableCollection<Control>>(nameof(ActionButtons));

    public static readonly StyledProperty<bool> ContentScrollEnabledProperty =
        AvaloniaProperty.Register<MessageBox, bool>(nameof(ContentScrollEnabled), true);

    public static readonly StyledProperty<bool> ShowCardProperty =
        AvaloniaProperty.Register<MessageBox, bool>(nameof(ShowCard), true);
    
    public static readonly StyledProperty<bool> ShowIconProperty =
        AvaloniaProperty.Register<MessageBox, bool>(nameof(ShowIcon), false);

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public ObservableCollection<Control> ActionButtons
    {
        get => GetValue(ActionButtonsProperty);
        set => SetValue(ActionButtonsProperty, value);
    }

    public bool ContentScrollEnabled
    {
        get => GetValue(ContentScrollEnabledProperty);
        set => SetValue(ContentScrollEnabledProperty, value);
    }

    public bool ShowCard
    {
        get => GetValue(ShowCardProperty);
        set => SetValue(ShowCardProperty, value);
    }
    
    public bool ShowIcon
    {
        get => GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    public MessageBox()
    {
        InitializeComponent();
    }
}