using Avalonia.Controls;
using System.Threading;
using System.Threading.Tasks;
using TableConverter.Views.Controls.Dialog.Options;
using TableConverter.Views.Controls.OverlayShared.Enums;

namespace TableConverter.Interfaces.OverlayService
{
    public interface IDialogBuilder
    {
        public IDialogBuilder WithView(Control control);

        public IDialogBuilder WithViewModel(object viewModel);

        public IDialogBuilder WithHost(string hostId);

        public IDialogBuilder WithOptions(OverlayDialogOptions options);

        public void Show();

        public Task<DialogResult> ShowAsync(CancellationToken? token = null);
    }
}
