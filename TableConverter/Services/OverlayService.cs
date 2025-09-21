using Avalonia.Controls;
using System.Threading;
using System.Threading.Tasks;
using TableConverter.Interfaces.OverlayService;
using TableConverter.Views.Controls.Dialog;
using TableConverter.Views.Controls.Dialog.Options;
using TableConverter.Views.Controls.Drawer;
using TableConverter.Views.Controls.Drawer.Options;
using TableConverter.Views.Controls.MessageBox;
using TableConverter.Views.Controls.MessageBox.Enums;
using TableConverter.Views.Controls.OverlayShared.Enums;

namespace TableConverter.Services
{
    public class OverlayService : IOverlayService
    {
        public IDialogBuilder CreateDialog() => new DialogBuilder();

        public ICustomDialogBuilder CreateCustomDialog() => new CustomDialogBuilder();

        public IMessageBoxBuilder CreateMessageBox() => new MessageBoxBuilder();

        public IDrawerBuilder CreateDrawer() => new DrawerBuilder();

        public ICustomDrawerBuilder CreateCustomDrawer() => new CustomDrawerBuilder();

        internal class DialogBuilder : IDialogBuilder
        {
            private Control? _view;
            private object? _viewModel;
            private string? _hostId;
            private OverlayDialogOptions? _options;

            public IDialogBuilder WithView(Control control)
            {
                _view = control;
                return this;
            }

            public IDialogBuilder WithViewModel(object viewModel)
            {
                _viewModel = viewModel;
                return this;
            }

            public IDialogBuilder WithHost(string hostId)
            {
                _hostId = hostId;
                return this;
            }

            public IDialogBuilder WithOptions(OverlayDialogOptions options)
            {
                _options = options;
                return this;
            }

            public void Show()
            {
                if (_view is not null)
                    OverlayDialog.Show(_view, _viewModel, _hostId, _options);
                else
                    OverlayDialog.Show(_viewModel, _hostId, _options);
            }

            public Task<DialogResult> ShowAsync(CancellationToken? token = null)
            {
                if (_view is not null)
                    return OverlayDialog.ShowModal(_view, _viewModel, _hostId, _options, token);
                else
                    return OverlayDialog.ShowModal(_viewModel, _hostId, _options, token);
            }
        }

        internal class CustomDialogBuilder : ICustomDialogBuilder
        {
            private Control? _view;
            private object? _viewModel;
            private string? _hostId;
            private OverlayDialogOptions? _options;

            public ICustomDialogBuilder WithView(Control control)
            {
                _view = control;
                return this;
            }

            public ICustomDialogBuilder WithViewModel(object viewModel)
            {
                _viewModel = viewModel;
                return this;
            }

            public ICustomDialogBuilder WithHost(string hostId)
            {
                _hostId = hostId;
                return this;
            }

            public ICustomDialogBuilder WithOptions(OverlayDialogOptions options)
            {
                _options = options;
                return this;
            }

            public void Show()
            {
                if (_view is not null)
                    OverlayDialog.ShowCustom(_view, _viewModel, _hostId, _options);
                else
                    OverlayDialog.ShowCustom(_viewModel, _hostId, _options);
            }

            public Task<TResult?> ShowAsync<TResult>(CancellationToken? token = null)
            {
                if (_view is not null)
                    return OverlayDialog.ShowCustomModal<TResult>(_view, _viewModel, _hostId, _options, token);
                else
                    return OverlayDialog.ShowCustomModal<TResult>(_viewModel, _hostId, _options, token);
            }
        }

        internal class MessageBoxBuilder : IMessageBoxBuilder
        {
            private string _message = string.Empty;
            private string? _title;
            private string? _hostId;
            private MessageBoxIcon _icon = MessageBoxIcon.None;
            private MessageBoxButton _buttons = MessageBoxButton.Ok;
            private int? _toplevelHash;
            private string? _styleClass;

            public IMessageBoxBuilder WithMessage(string message)
            {
                _message = message;
                return this;
            }

            public IMessageBoxBuilder WithTitle(string title)
            {
                _title = title;
                return this;
            }

            public IMessageBoxBuilder WithHost(string hostId)
            {
                _hostId = hostId;
                return this;
            }

            public IMessageBoxBuilder WithIcon(MessageBoxIcon icon)
            {
                _icon = icon;
                return this;
            }

            public IMessageBoxBuilder WithButtons(MessageBoxButton buttons)
            {
                _buttons = buttons;
                return this;
            }

            public IMessageBoxBuilder WithTopLevelHash(int hash)
            {
                _toplevelHash = hash;
                return this;
            }

            public IMessageBoxBuilder WithStyleClass(string styleClass)
            {
                _styleClass = styleClass;
                return this;
            }

            public async Task<MessageBoxResult> ShowAsync()
            {
                return await MessageBox.ShowOverlayAsync(
                    _message,
                    _title,
                    _hostId,
                    _icon,
                    _buttons,
                    _toplevelHash,
                    _styleClass
                );
            }
        }

        internal class DrawerBuilder : IDrawerBuilder
        {
            private Control? _view;
            private object? _viewModel;
            private string? _hostId;
            private DrawerOptions? _options;

            public IDrawerBuilder WithView(Control control)
            {
                _view = control;
                return this;
            }

            public IDrawerBuilder WithViewModel(object viewModel)
            {
                _viewModel = viewModel;
                return this;
            }

            public IDrawerBuilder WithHost(string hostId)
            {
                _hostId = hostId;
                return this;
            }

            public IDrawerBuilder WithOptions(DrawerOptions options)
            {
                _options = options;
                return this;
            }

            public void Show()
            {
                if (_view is not null)
                    Drawer.Show(_view, _viewModel, _hostId, _options);
                else
                    Drawer.Show(_viewModel, _hostId, _options);
            }

            public Task<DialogResult> ShowAsync()
            {
                if (_view is not null)
                    return Drawer.ShowModal(_view, _viewModel, _hostId, _options);
                else
                    return Drawer.ShowModal(_viewModel, _hostId, _options);
            }
        }

        internal class CustomDrawerBuilder : ICustomDrawerBuilder
        {
            private Control? _view;
            private object? _viewModel;
            private string? _hostId;
            private DrawerOptions? _options;

            public ICustomDrawerBuilder WithView(Control control)
            {
                _view = control;
                return this;
            }

            public ICustomDrawerBuilder WithViewModel(object viewModel)
            {
                _viewModel = viewModel;
                return this;
            }

            public ICustomDrawerBuilder WithHost(string hostId)
            {
                _hostId = hostId;
                return this;
            }

            public ICustomDrawerBuilder WithOptions(DrawerOptions options)
            {
                _options = options;
                return this;
            }

            public void Show()
            {
                if (_view is not null)
                    Drawer.ShowCustom(_view, _viewModel, _hostId, _options);
                else
                    Drawer.ShowCustom(_viewModel, _hostId, _options);
            }

            public Task<TResult?> ShowAsync<TResult>()
            {
                if (_view is not null)
                    return Drawer.ShowCustomModal<TResult>(_view, _viewModel, _hostId, _options);
                else
                    return Drawer.ShowCustomModal<TResult>(_viewModel, _hostId, _options);
            }
        }
    }
}
