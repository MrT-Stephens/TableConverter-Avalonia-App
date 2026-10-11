using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Media;
using TableConverter.Commands.DataModels;

namespace TableConverter.Converters;

/// <summary>
///     Builds the main menu from a workspace's <see cref="CommandMenu" />. The menu itself decides which
///     groups there are, the order they run in and where the separators fall, so this only turns that
///     description into the <see cref="MenuItem" /> tree the menu bar draws.
/// </summary>
public class CommandMenuToMenuItemsConverter : IValueConverter
{
    public static readonly CommandMenuToMenuItemsConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not CommandMenu menu)
        {
            return new BindingNotification("Value must be a command menu");
        }

        return BuildMenu(menu);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static AvaloniaList<MenuItem> BuildMenu(CommandMenu menu)
    {
        var result = new AvaloniaList<MenuItem>();

        foreach (var group in menu.Groups)
        {
            var groupItem = new MenuItem
            {
                Header = group.Title
            };

            var menuChildren = new List<object>();

            for (var i = 0; i < group.Sections.Count; i++)
            {
                menuChildren.AddRange(CreateMenuItems(group.Sections[i]));

                // add separator except after last section
                if (i < group.Sections.Count - 1)
                {
                    menuChildren.Add(new Separator());
                }
            }

            groupItem.ItemsSource = menuChildren;
            result.Add(groupItem);
        }

        return result;
    }

    private static IEnumerable<MenuItem> CreateMenuItems(CommandMenuSection section)
    {
#if DEBUG
        var sectionCommands = section.Commands.ToList();
#endif

        foreach (var cmd in section.Commands)
        {
            var keyGesture = cmd.Metadata.KeyGestures.FirstOrDefault();

            if (keyGesture is not null)
            {
#if DEBUG
                var anyWithGesture = sectionCommands
                    .Where(ci => ci.Metadata.Name != cmd.Metadata.Name)
                    .Any(ci => ci.Metadata.KeyGestures.Contains(keyGesture));

                Debug.Assert(!anyWithGesture, $"Duplicate key gesture '{keyGesture}' found in command '{cmd.Metadata.Title}'.");
#endif

                if (OperatingSystem.IsMacOS())
                {
                    // Convert Ctrl to Cmd on macOS
                    keyGesture = keyGesture.Replace("Ctrl", "Cmd");
                }
                else if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
                {
                    // Convert Cmd to Ctrl on Windows/Linux
                    keyGesture = keyGesture.Replace("Cmd", "Ctrl");
                }
            }

            var item = new MenuItem
            {
                Header = cmd.Metadata.Title,
                Command = cmd.Command,
                Icon = new PathIcon { Data = cmd.Metadata.IconPath },
            };

            if (keyGesture is not null)
            {
                var gesture = KeyGesture.Parse(keyGesture);

                item.InputGesture = gesture;
                item.HotKey = gesture;
            }

            ToolTip.SetShowDelay(item, 1000);
            ToolTip.SetBetweenShowDelay(item, 500);
            ToolTip.SetPlacement(item, PlacementMode.Right);
            ToolTip.SetTip(item, new ContentControl
            {
                Content = new TextBlock
                {
                    Text = $"{cmd.Metadata.Title}: {cmd.Metadata.Description}",
                    TextWrapping = TextWrapping.Wrap,
                },
                MaxWidth = 300,
            });

            yield return item;
        }
    }
}
