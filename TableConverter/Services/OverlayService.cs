using Avalonia.Controls;
using System.Threading;
using System.Threading.Tasks;
using TableConverter.Interfaces.OverlayService;
using TableConverter.Views.Controls.Dialog;
using TableConverter.Views.Controls.Dialog.Options;
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

        private class DialogBuilder : IDialogBuilder
        {
            private Control? _View;
            private object? _ViewModel;
            private string? _HostId;
            private OverlayDialogOptions? _Options;

            public IDialogBuilder WithView(Control control)
            {
                _View = control;
                return this;
            }

            public IDialogBuilder WithViewModel(object viewModel)
            {
                _ViewModel = viewModel;
                return this;
            }

            public IDialogBuilder WithHost(string hostId)
            {
                _HostId = hostId;
                return this;
            }

            public IDialogBuilder WithOptions(OverlayDialogOptions options)
            {
                _Options = options;
                return this;
            }

            public void Show()
            {
                if (_View is not null)
                    OverlayDialog.Show(_View, _ViewModel, _HostId, _Options);
                else
                    OverlayDialog.Show(_ViewModel, _HostId, _Options);
            }

            public Task<DialogResult> ShowAsync(CancellationToken? token = null)
            {
                if (_View is not null)
                    return OverlayDialog.ShowModal(_View, _ViewModel, _HostId, _Options, token);
                else
                    return OverlayDialog.ShowModal(_ViewModel, _HostId, _Options, token);
            }
        }

        private class CustomDialogBuilder : ICustomDialogBuilder
        {
            private Control? _View;
            private object? _ViewModel;
            private string? _HostId;
            private OverlayDialogOptions? _Options;

            public ICustomDialogBuilder WithView(Control control)
            {
                _View = control;
                return this;
            }

            public ICustomDialogBuilder WithViewModel(object viewModel)
            {
                _ViewModel = viewModel;
                return this;
            }

            public ICustomDialogBuilder WithHost(string hostId)
            {
                _HostId = hostId;
                return this;
            }

            public ICustomDialogBuilder WithOptions(OverlayDialogOptions options)
            {
                _Options = options;
                return this;
            }

            public void Show()
            {
                if (_View is not null)
                    OverlayDialog.ShowCustom(_View, _ViewModel, _HostId, _Options);
                else
                    OverlayDialog.ShowCustom(_ViewModel, _HostId, _Options);
            }

            public Task<TResult?> ShowAsync<TResult>(CancellationToken? token = null)
            {
                if (_View is not null)
                    return OverlayDialog.ShowCustomModal<TResult>(_View, _ViewModel, _HostId, _Options, token);
                else
                    return OverlayDialog.ShowCustomModal<TResult>(_ViewModel, _HostId, _Options, token);
            }
        }

        private class MessageBoxBuilder : IMessageBoxBuilder
        {
            private string _Message = string.Empty;
            private string? _Title;
            private string? _HostId;
            private MessageBoxIcon _Icon = MessageBoxIcon.None;
            private MessageBoxButton _Buttons = MessageBoxButton.Ok;
            private int? _ToplevelHash;
            private string? _StyleClass;

            public IMessageBoxBuilder WithMessage(string message)
            {
                _Message = message;
                return this;
            }

            public IMessageBoxBuilder WithTitle(string title)
            {
                _Title = title;
                return this;
            }

            public IMessageBoxBuilder WithHost(string hostId)
            {
                _HostId = hostId;
                return this;
            }

            public IMessageBoxBuilder WithIcon(MessageBoxIcon icon)
            {
                _Icon = icon;
                return this;
            }

            public IMessageBoxBuilder WithButtons(MessageBoxButton buttons)
            {
                _Buttons = buttons;
                return this;
            }

            public IMessageBoxBuilder WithTopLevelHash(int hash)
            {
                _ToplevelHash = hash;
                return this;
            }

            public IMessageBoxBuilder WithStyleClass(string styleClass)
            {
                _StyleClass = styleClass;
                return this;
            }

            public async Task<MessageBoxResult> ShowAsync()
            {
                return await MessageBox.ShowOverlayAsync(
                    _Message,
                    _Title,
                    _HostId,
                    _Icon,
                    _Buttons,
                    _ToplevelHash,
                    _StyleClass
                );
            }
        }
    }
}
