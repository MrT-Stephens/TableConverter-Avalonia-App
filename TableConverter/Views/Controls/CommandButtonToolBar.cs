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

    /// <summary>
    ///     The command collection currently shown, so a change of <see cref="Commands" /> can be followed and the
    ///     collection is never subscribed to more than once.
    /// </summary>
    private ObservableCollection<ICommandInstance>? _ShownCommands;

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

        // The commands are exposed as a property of this control rather than as the items source, so they are
        // taken up here and followed from then on. A view which binds the items source itself, rather than this
        // property, is left alone until there are commands to show so its binding is not replaced with nothing.
        ShowCommands(Commands);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        // The commands are usually bound to a view model, and a binding is resolved again once the data context
        // arrives, so a change of Commands has to be followed and not just the value read at initialisation.
        if (change.Property == CommandsProperty)
        {
            ShowCommands(change.GetNewValue<ObservableCollection<ICommandInstance>?>());
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // Leaving the tree releases the collection subscription, so it is taken up again on the way back in.
        ShowCommands(Commands);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (_ShownCommands is not null)
        {
            _ShownCommands.CollectionChanged -= OnCommandsCollectionChanged;
            _ShownCommands = null;
        }

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

    /// <summary>
    ///     Makes <paramref name="commands" /> the items of the toolbar. A null collection (a view model which has
    ///     not been set yet) leaves whatever the view bound to the items source in place.
    /// </summary>
    private void ShowCommands(ObservableCollection<ICommandInstance>? commands)
    {
        if (commands is null || ReferenceEquals(_ShownCommands, commands))
        {
            return;
        }

        if (_ShownCommands is not null)
        {
            _ShownCommands.CollectionChanged -= OnCommandsCollectionChanged;
        }

        _ShownCommands = commands;
        commands.CollectionChanged += OnCommandsCollectionChanged;

        // Assigning the items source replaces any binding a view made to it, which is why the commands of a view
        // that binds this property have to be adopted here rather than by that binding.
        ItemsSource = commands;

        foreach (var command in commands)
        {
            Wire(command);
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
            _ShownCommands?.Any(c => c.Context.IsProcessing) == true;
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

            if (_ShownCommands is not null)
                foreach (var cmd in _ShownCommands)
                    Wire(cmd);
        }

        UpdateAnyProcessing();
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
