using Avalonia.Interactivity;
using TableConverter.Views.Controls.Dialog.Enums;

namespace TableConverter.Views.Controls.Dialog.Events;

public class DialogLayerChangeEventArgs : RoutedEventArgs
{
    public DialogLayerChangeType ChangeType { get; }

    public DialogLayerChangeEventArgs(DialogLayerChangeType type)
    {
        ChangeType = type;
    }

    public DialogLayerChangeEventArgs(RoutedEvent routedEvent, DialogLayerChangeType type) : base(routedEvent)
    {
        ChangeType = type;
    }
}