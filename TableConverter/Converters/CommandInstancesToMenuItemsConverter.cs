using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Media;
using TableConverter.Commands.Interfaces;
using TableConverter.Utilities.Extensions;

namespace TableConverter.Converters;

public class CommandInstancesToMenuItemsConverter : IValueConverter
{
    public static readonly CommandInstancesToMenuItemsConverter Instance = new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IEnumerable<ICommandInstance> commandInstances)
        {
            return new BindingNotification("Value must be a collection of command instances");
        }

        return BuildMenu(commandInstances);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static AvaloniaList<MenuItem> BuildMenu(IEnumerable<ICommandInstance> commandInstances)
    {
        var result = new AvaloniaList<MenuItem>();
        
        var categories = commandInstances
            .GroupBy(x => x.Metadata.Category)
            .OrderBy(g => g.Key);

        foreach (var category in categories)
        {
            var categoryItem = new MenuItem
            {
                Header = category.Key
            };
            
            var subcategories = category
                .GroupBy(x => x.Metadata.SubCategoryIndex)
                .OrderBy(g => g.Key)
                .ToList();

            var menuChildren = new List<object>();

            for (var i = 0; i < subcategories.Count; i++)
            {
                var subItems = subcategories[i];

                // add commands inside subcategory
                var subMenuItems = subItems
                    .OrderBy(x => x.Metadata.Title)
                    .Select(cmd =>
                    {
                        var gesture = KeyGesture.Parse(cmd.Metadata.KeyGestures.First());

                        var item = new MenuItem
                        {
                            Header = cmd.Metadata.Title,
                            Command = cmd.Command,
                            InputGesture = gesture,
                            HotKey = gesture,
                            Icon = new PathIcon { Data = cmd.Metadata.IconPath },
                        };
                        
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

                        return item;
                    });

                menuChildren.AddRange(subMenuItems);

                // add separator except after last group
                if (i < subcategories.Count - 1)
                {
                    menuChildren.Add(new Separator());
                }
            }

            categoryItem.ItemsSource = menuChildren;
            result.Add(categoryItem);
        }

        return result;
    }
}