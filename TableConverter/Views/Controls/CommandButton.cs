using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using SukiUI.Theme;
using TableConverter.Commands.Interfaces;

namespace TableConverter.Views.Controls
{
    public class CommandButton : Button
    {
        protected override Type StyleKeyOverride => typeof(Button);

        public static readonly StyledProperty<ICommandInstance?> CommandInstanceProperty =
            AvaloniaProperty.Register<CommandButton, ICommandInstance?>(nameof(CommandInstance));

        public static readonly StyledProperty<Orientation> OrientationProperty =
            AvaloniaProperty.Register<CommandButton, Orientation>(
                nameof(Orientation), Orientation.Horizontal);

        public static readonly StyledProperty<bool> ShowTextProperty =
            AvaloniaProperty.Register<CommandButton, bool>(
                nameof(ShowText), true);

        public static readonly StyledProperty<bool> ShowIconProperty =
            AvaloniaProperty.Register<CommandButton, bool>(
                nameof(ShowIcon), true);

        public ICommandInstance? CommandInstance
        {
            get => GetValue(CommandInstanceProperty);
            set => SetValue(CommandInstanceProperty, value);
        }

        public Orientation Orientation
        {
            get => GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
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

        /// <summary>
        ///     The width the spinner the theme shows while a command is busy grows to, which is what it takes
        ///     from the room the title has once it appears.
        /// </summary>
        private const double ProgressRoom = 40;

        /// <summary>
        ///     The panel holding the icon and the text, which is what this button hands the theme as its content.
        /// </summary>
        private StackPanel? _layout;

        /// <summary>
        ///     The title shown on the button, kept hold of so that the room it is given can be kept up to date.
        /// </summary>
        private TextBlock? _text;

        /// <summary>
        ///     The box the icon sits in, the width of which is what the title gives up room for.
        /// </summary>
        private Viewbox? _iconBox;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            this[!CommandProperty] = new Binding
            {
                Path = string.Join('.', nameof(CommandInstance), 
                                        nameof(ICommandInstance.Command)),
                Source = this,
                Mode = BindingMode.OneWay,
            };

            var text = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Text = CommandInstance?.Metadata.Title,
                TextTrimming = TextTrimming.CharacterEllipsis,
                TextWrapping = TextWrapping.NoWrap,
                [!IsVisibleProperty] = new Binding
                {
                    Path = nameof(ShowText),
                    Source = this,
                    Mode = BindingMode.OneWay,
                }
            };

            var icon = new PathIcon
            {
                [!PathIcon.DataProperty] = new Binding
                {
                    Path = nameof(ICommandMetadata.IconPath),
                    Source = CommandInstance!.Metadata,
                    Mode = BindingMode.OneWay,
                },
            };

            var iconBox = new Viewbox
            {
                Child = icon,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                [!Layoutable.HeightProperty] = new Binding
                {
                    Path = nameof(FontSize),
                    Source = this,
                    Mode = BindingMode.OneWay,
                },
                [!IsVisibleProperty] = new Binding
                {
                    Path = nameof(ShowIcon),
                    Source = this,
                    Mode = BindingMode.OneWay,
                },
            };

            // The theme in use always lays an icon out to the left of the content, so the icon and the text are
            // paired here and handed over as the content instead. That is what lets the orientation asked of the
            // button, the two either beside one another or one above the other, be the one that is shown.
            _layout = new StackPanel
            {
                [!Layoutable.HorizontalAlignmentProperty] = new Binding
                {
                    Path = nameof(HorizontalContentAlignment),
                    Source = this,
                    Mode = BindingMode.OneWay,
                },
            };
            _layout.Children.Add(iconBox);
            _layout.Children.Add(text);
            Content = _layout;

            _text = text;
            _iconBox = iconBox;

            ApplyOrientation();

            this[!ButtonExtensions.ShowProgressProperty] = new Binding
            {
                Path = nameof(ICommandContext.IsLoading),
                Source = CommandInstance!.Context,
                Mode = BindingMode.OneWay,
            };
            
            ToolTip.SetShowDelay(this, 1000);
            ToolTip.SetBetweenShowDelay(this, 500);
            ToolTip.SetTip(this, new TextBlock
            {
                Text = $"{CommandInstance?.Metadata.Title}: {CommandInstance?.Metadata.Description}",
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 300,
            });
            
            Padding = new Thickness(8);
            Classes.Add("Flat");
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            // The orientation is normally bound to the toolbar the button sits in, so it can arrive after the
            // button has been built, and it can be changed afterwards too.
            if (change.Property == OrientationProperty)
            {
                ApplyOrientation();
            }

            // Anything which changes the room the title has to fit into is worth reacting to, the width the
            // button has been given above all.
            if (change.Property == BoundsProperty ||
                change.Property == PaddingProperty ||
                change.Property == BorderThicknessProperty ||
                change.Property == FontSizeProperty ||
                change.Property == ShowIconProperty ||
                change.Property == ButtonExtensions.ShowProgressProperty)
            {
                UpdateTextRoom();
            }
        }

        /// <summary>
        ///     Pairs the icon and the text up the way the button is asked to, either beside one another or one
        ///     above the other.
        /// </summary>
        private void ApplyOrientation()
        {
            if (_layout is null)
            {
                return;
            }

            var vertical = Orientation == Orientation.Vertical;

            _layout.Orientation = vertical ? Orientation.Vertical : Orientation.Horizontal;
            _layout.Spacing = 10;

            UpdateTextRoom();
        }

        /// <summary>
        ///     Limits the title to the room the button actually has. The theme lays the content out in a
        ///     stack, and a stack measures whatever it holds with as much room as it asks for, so without a
        ///     limit of its own a long title would be left to run past the edge of the button rather than be
        ///     trimmed.
        /// </summary>
        private void UpdateTextRoom()
        {
            if (_text is null || _layout is null)
            {
                return;
            }

            // Until the button has been given a width there is nothing to work from, so the title is left
            // alone rather than being squeezed away on the first pass.
            if (Bounds.Width <= 0)
            {
                _text.MaxWidth = double.PositiveInfinity;
                return;
            }

            var room = Bounds.Width - Padding.Left - Padding.Right
                                   - BorderThickness.Left - BorderThickness.Right;

            // The spinner shown while the command is busy stands ahead of everything else, so what it takes
            // comes off the room before the icon does.
            if (ButtonExtensions.GetShowProgress(this))
            {
                room -= ProgressRoom;
            }

            // An icon sitting beside the title takes its width and the gap after it away from the room left.
            if (Orientation == Orientation.Horizontal && ShowIcon)
            {
                var iconWidth = _iconBox is { Bounds.Width: > 0 } ? _iconBox.Bounds.Width : FontSize;
                room -= iconWidth + _layout.Spacing;
            }

            _text.MaxWidth = Math.Max(0, room);
        }
    }
}
