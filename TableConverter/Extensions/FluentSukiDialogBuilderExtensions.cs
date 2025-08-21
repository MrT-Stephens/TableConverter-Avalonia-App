using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using SukiUI.Dialogs;
using TableConverter.Components.Extensions;

namespace TableConverter.Extensions;

public static class FluentSukiDialogBuilderExtensions
{
    public static SukiDialogBuilder WithSelection<TItem>(this SukiDialogBuilder builder,
        IEnumerable<TItem> items, Action<TItem?> onSelectedItemChanged, int columns = 4)
        where TItem : notnull
    {
        var ctrl = new GridListBox
        {
            Spacing = 10,
            Columns = columns,
            ItemsSource = items
        };
        
        ctrl.SelectionChanged += (_, args) =>
        {
            if (args.AddedItems.Count > 0)
            {
                onSelectedItemChanged((TItem?)args.AddedItems[0]);
            }
        };
        
        return builder.WithContent(ctrl);
    }
    
    public static Task<bool> TryShowErrorDialogAsync(this SukiDialogBuilder builder, string title, string? message = null)
    {
        return builder
            .WithTitle(title)
            .WithContent(message ?? "An unexpected error occurred.")
            .OfType(NotificationType.Error)
            .Dismiss().ByClickingBackground()
            .TryShowAsync();
    }
}