using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using TableConverter.Commands.Interfaces;

namespace TableConverter.Views.Controls;

public class CommandButtonToolBar : ItemsControl
{
    protected override Type StyleKeyOverride => typeof(ItemsControl);

    public static readonly StyledProperty<ObservableCollection<ICommandInstance>?> CommandsProperty =
        AvaloniaProperty.Register<CommandButtonToolBar, ObservableCollection<ICommandInstance>?>(nameof(Commands));

    public static readonly StyledProperty<bool> ShowTextProperty =
        AvaloniaProperty.Register<CommandButtonToolBar, bool>(
            nameof(ShowText), true);

    public static readonly StyledProperty<bool> ShowIconProperty =
        AvaloniaProperty.Register<CommandButtonToolBar, bool>(
            nameof(ShowIcon), true);

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<CommandButtonToolBar, Orientation>(
            nameof(Orientation), Orientation.Horizontal);

    public ObservableCollection<ICommandInstance>? Commands
    {
        get => GetValue(CommandsProperty);
        set => SetValue(CommandsProperty, value);
    }

    public bool ShowText
    {
        get => GetValue(ShowTextProperty);
        set => SetValue(ShowTextProperty, value);
    }

    public bool ShowIcon
    {
        get => GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public CommandButtonToolBar()
    {
        // Default to a horizontal StackPanel
        ItemsPanel = new FuncTemplate<Panel>(() => new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
        })!;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        ItemsSource = Commands;
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var button = new CommandButton
        {
            [!CommandButton.CommandInstanceProperty] = new Binding
            {
                Path = ".",
                Source = item,
            },
            [!CommandButton.OrientationProperty] = new Binding
            {
                Source = this,
                Path = nameof(Orientation)
            },
            [!CommandButton.ShowTextProperty] = new Binding
            {
                Source = this,
                Path = nameof(ShowText)
            },
            [!CommandButton.ShowIconProperty] = new Binding
            {
                Source = this,
                Path = nameof(ShowIcon)
            }
        };

        return button;
    }
}
