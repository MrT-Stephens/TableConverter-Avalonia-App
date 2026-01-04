using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using TableConverter.Commands.Interfaces;
using TableConverter.Views.Controls.Converters;

namespace TableConverter.Views.Controls;

public class CommandButtonToolBar : ItemsControl
{
    protected override Type StyleKeyOverride => typeof(ItemsControl);

    public static readonly StyledProperty<ObservableCollection<ICommandInstance>?> CommandsProperty =
        AvaloniaProperty.Register<CommandButtonToolBar, ObservableCollection<ICommandInstance>?>(nameof(Commands));

    public static readonly StyledProperty<bool> ShowTextProperty =
        AvaloniaProperty.Register<CommandButtonToolBar, bool>(nameof(ShowText), true);

    public static readonly StyledProperty<bool> ShowIconProperty =
        AvaloniaProperty.Register<CommandButtonToolBar, bool>(nameof(ShowIcon), true);

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<CommandButtonToolBar, Orientation>(
            nameof(Orientation), Orientation.Horizontal);

    public static readonly StyledProperty<bool> DisableAllIfProcessingProperty =
        AvaloniaProperty.Register<CommandButtonToolBar, bool>(
            nameof(DisableAllIfProcessing), true);

    public static readonly StyledProperty<bool> AnyCommandProcessingProperty =
        AvaloniaProperty.Register<CommandButtonToolBar, bool>(
            nameof(AnyCommandProcessing));

    public bool AnyCommandProcessing
    {
        get => GetValue(AnyCommandProcessingProperty);
        set => SetValue(AnyCommandProcessingProperty, value);
    }

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

    public bool DisableAllIfProcessing
    {
        get => GetValue(DisableAllIfProcessingProperty);
        set => SetValue(DisableAllIfProcessingProperty, value);
    }

    private readonly HashSet<ICommandInstance> _wiredCommands = [];

    public CommandButtonToolBar()
    {
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

        if (Commands is not null)
        {
            Commands.CollectionChanged += OnCommandsCollectionChanged;

            foreach (var cmd in Commands)
            {
                Wire(cmd);
            }

            UpdateAnyProcessing();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (Commands != null)
            Commands.CollectionChanged -= OnCommandsCollectionChanged;

        foreach (var cmd in _wiredCommands)
        {
            if (cmd.Context is not INotifyPropertyChanged notify)
            {
                continue;
            }
            
            notify.PropertyChanged -= OnCommandMetadataChanged;
        }

        _wiredCommands.Clear();
    }

    private void OnCommandsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (ICommandInstance cmd in e.OldItems)
            {
                Unwire(cmd);
            }
        }

        if (e.NewItems != null)
        {
            foreach (ICommandInstance cmd in e.NewItems)
            {
                Wire(cmd);
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            foreach (var cmd in _wiredCommands)
            {
                if (cmd.Context is not INotifyPropertyChanged notify)
                {
                    continue;
                }
                
                notify.PropertyChanged -= OnCommandMetadataChanged;
            }

            _wiredCommands.Clear();

            if (Commands != null)
                foreach (var cmd in Commands)
                    Wire(cmd);
        }

        UpdateAnyProcessing();
    }

    private void Wire(ICommandInstance cmd)
    {
        if (cmd.Context is not INotifyPropertyChanged notify)
        {
            return;
        }
        
        if (_wiredCommands.Add(cmd))
        {
            notify.PropertyChanged += OnCommandMetadataChanged;
        }
    }

    private void Unwire(ICommandInstance cmd)
    {
        if (cmd.Context is not INotifyPropertyChanged notify)
        {
            return;
        }
        
        if (_wiredCommands.Remove(cmd))
        {
            notify.PropertyChanged -= OnCommandMetadataChanged;
        }
    }

    private void OnCommandMetadataChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ICommandContext.IsProcessing))
        {
            UpdateAnyProcessing();
        }
    }

    private void UpdateAnyProcessing()
    {
        AnyCommandProcessing =
            DisableAllIfProcessing &&
            Commands?.Any(c => c.Context.IsProcessing) == true;
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
            },
            [!IsEnabledProperty] = new MultiBinding
            {
                Converter = new DisableIfProcessingMultiConverter(),
                Bindings =
                {
                    new Binding
                    {
                        Source = this, 
                        Path = nameof(DisableAllIfProcessing)
                    },
                    new Binding
                    {
                        Source = this, 
                        Path = nameof(AnyCommandProcessing)
                    }
                }
            }
        };

        return button;
    }
}
