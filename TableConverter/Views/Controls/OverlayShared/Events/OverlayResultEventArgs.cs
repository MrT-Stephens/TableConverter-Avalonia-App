using Avalonia.Interactivity;

namespace TableConverter.Views.Controls.OverlayShared.Events;

public class OverlayResultEventArgs : RoutedEventArgs
{
    public object? Result { get; set; }

    public OverlayResultEventArgs(object? result)
    {
        Result = result;
    }

    public OverlayResultEventArgs(RoutedEvent routedEvent, object? result) : base(routedEvent)
    {
        Result = result;
    }
}