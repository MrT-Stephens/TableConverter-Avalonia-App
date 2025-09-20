using Avalonia.Controls;
using System.Threading;
using System.Threading.Tasks;
using TableConverter.Views.Controls.Drawer.Options;
using TableConverter.Views.Controls.OverlayShared.Enums;

namespace TableConverter.Interfaces.OverlayService
{
    public interface IDrawerBuilder
    {
        public IDrawerBuilder WithView(Control control);

        public IDrawerBuilder WithViewModel(object viewModel);

        public IDrawerBuilder WithHost(string hostId);

        public IDrawerBuilder WithOptions(DrawerOptions options);

        public void Show();

        public Task<DialogResult> ShowAsync();
    }
}
