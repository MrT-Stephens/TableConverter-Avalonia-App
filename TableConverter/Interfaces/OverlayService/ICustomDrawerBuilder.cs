using Avalonia.Controls;
using System.Threading.Tasks;
using TableConverter.Views.Controls.Drawer.Options;

namespace TableConverter.Interfaces.OverlayService
{
    public interface ICustomDrawerBuilder
    {
        public ICustomDrawerBuilder WithView(Control control);

        public ICustomDrawerBuilder WithViewModel(object viewModel);

        public ICustomDrawerBuilder WithHost(string hostId);

        public ICustomDrawerBuilder WithOptions(DrawerOptions options);

        public void Show();

        public Task<TResult?> ShowAsync<TResult>();
    }
}
