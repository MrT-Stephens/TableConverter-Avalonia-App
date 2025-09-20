using Avalonia.Controls;
using System.Threading;
using System.Threading.Tasks;
using TableConverter.Views.Controls.Dialog.Options;

namespace TableConverter.Interfaces.OverlayService
{
    public interface ICustomDrawerBuilder
    {
        public ICustomDialogBuilder WithView(Control control);

        public ICustomDialogBuilder WithViewModel(object viewModel);

        public ICustomDialogBuilder WithHost(string hostId);

        public ICustomDialogBuilder WithOptions(OverlayDialogOptions options);

        public void Show();

        public Task<TResult?> ShowAsync<TResult>(CancellationToken? token = null);
    }
}
