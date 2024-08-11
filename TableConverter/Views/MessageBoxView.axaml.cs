using Avalonia;
using Avalonia.Controls;
using System.Collections.ObjectModel;

namespace TableConverter.Views;

public partial class MessageBoxView : UserControl
{
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<MessageBoxView, object?>(nameof(Icon));

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<MessageBoxView, string>(nameof(Title));

    public static readonly StyledProperty<ObservableCollection<Control>> ActionButtonsProperty =
        AvaloniaProperty.Register<MessageBoxView, ObservableCollection<Control>>(nameof(ActionButtons));

    public static readonly StyledProperty<bool> ContentScrollEnabledProperty =
        AvaloniaProperty.Register<MessageBoxView, bool>(nameof(ContentScrollEnabled), true);

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

    public MessageBoxView()
    {
        InitializeComponent();
    }
}