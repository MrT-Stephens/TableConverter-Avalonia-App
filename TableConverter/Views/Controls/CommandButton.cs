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

            Content =
                new TextBlock
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
            
            ButtonExtensions.SetIcon(this, new PathIcon
            {
                Data = CommandInstance?.Metadata.IconPath
            });
            
            ToolTip.SetShowDelay(this, 1000);
            ToolTip.SetBetweenShowDelay(this, 500);
            ToolTip.SetTip(this, new ContentControl
            {
                Content = new TextBlock
                {
                    Text = $"{CommandInstance?.Metadata.Title}: {CommandInstance?.Metadata.Description}",
                    TextWrapping = TextWrapping.Wrap,
                },
                MaxWidth = 300,
            });
            
            Padding = new Thickness(8);
            Classes.Add("Flat");
        }
    }
}
