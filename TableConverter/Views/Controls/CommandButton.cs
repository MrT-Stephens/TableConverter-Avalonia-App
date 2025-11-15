using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Layout;
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
            
            Content = new StackPanel
            {
                Orientation = Orientation,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 5,
                Children =
                {
                    new Path
                    {
                        Width = 16,
                        Height = 16,
                        Stretch = Avalonia.Media.Stretch.Uniform,
                        [!Path.DataProperty] = new Binding
                        {
                            Path = string.Join('.', nameof(CommandInstance), 
                                                    nameof(ICommandInstance.Metadata),
                                                    nameof(ICommandMetadata.IconPath)),
                            Source = this,
                            Mode = BindingMode.OneWay,
                        },
                        [!IsVisibleProperty] = new Binding
                        {
                            Path = nameof(ShowIcon),
                            Source = this,
                            Mode = BindingMode.OneWay,
                        }
                    },
                    new TextBlock
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        [!TextBlock.TextProperty] = new Binding
                        {
                            Path = string.Join('.', nameof(CommandInstance), 
                                                    nameof(ICommandInstance.Metadata),
                                                    nameof(ICommandMetadata.Title)),
                            Source = this,
                            Mode = BindingMode.OneWay,
                        },
                        [!IsVisibleProperty] = new Binding
                        {
                            Path = nameof(ShowText),
                            Source = this,
                            Mode = BindingMode.OneWay,
                        }
                    }
                }
            };
        }
    }
}
