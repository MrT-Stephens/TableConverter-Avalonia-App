using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Controls;
using SukiUI.Dialogs;

namespace TableConverter.Extensions;

public static class SukiDialogBuilderExtensions
{
    public static SukiDialogBuilder WithForm<TForm>(this SukiDialogBuilder builder, TForm form)
        where TForm : ObservableObject
    {
        builder.WithContent(new PropertyGrid
        {
            Item = form,
            DataTemplates =
            {
                new PropertyGridTemplateSelector
                {
                    UseSukiHost = true,
                }
            },
        });

        return builder;
    }
    
    public static SukiDialogBuilder WithSelection<TItem>(this SukiDialogBuilder builder,
        IEnumerable<TItem> items, Action<TItem?> onSelectedItemChanged, int columns = 4)
        where TItem : notnull
    {
        var ctrl = new ListBox
        {
            ItemsSource = items,
            SelectionMode = SelectionMode.AlwaysSelected | SelectionMode.Single,
            ItemsPanel = new FuncTemplate<Panel?>(() => new UniformGrid
            {
                Columns = columns,
                RowSpacing = 10,
                ColumnSpacing = 10,
            })
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

    public static SukiDialogBuilder WithContentList(this SukiDialogBuilder builder, 
        IEnumerable<Control> controls)
    {
        var ctrl = new ItemsControl()
        {
            ItemsSource = controls,
            ItemsPanel = new FuncTemplate<Panel?>(() => new StackPanel
            {
                Spacing = 10,
            }),
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