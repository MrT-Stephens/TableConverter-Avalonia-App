using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using TableConverter.Views.Controls.MessageBox.Enums;
using TableConverter.Views.Controls.OverlayShared;

namespace TableConverter.Views.Controls.MessageBox;

public static class MessageBox
{
    public static async Task<MessageBoxResult> ShowOverlayAsync(
        string message,
        string? title = null,
        string? hostId = null,
        MessageBoxIcon icon = MessageBoxIcon.None,
        MessageBoxButton button = MessageBoxButton.Ok,
        int? toplevelHashCode = null,
        string? styleClass = null)
    {
        var host = OverlayDialogManager.GetHost(hostId, toplevelHashCode);
        
        if (host is null) 
            return MessageBoxResult.None;
        
        var messageControl = new MessageBoxControl
        {
            Content = message,
            Title = title,
            Buttons = button,
            MessageIcon = icon,
            [KeyboardNavigation.TabNavigationProperty] = KeyboardNavigationMode.Cycle
        };
        
        if (!string.IsNullOrWhiteSpace(styleClass))
        {
            var styles = styleClass.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            messageControl.Classes.AddRange(styles);
        }

        host.AddModalDialog(messageControl);
        var result = await messageControl.ShowAsync<MessageBoxResult>();
        
        return result;
    }
}