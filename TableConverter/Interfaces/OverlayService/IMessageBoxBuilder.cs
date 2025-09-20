using System.Threading.Tasks;
using TableConverter.Views.Controls.MessageBox.Enums;

namespace TableConverter.Interfaces.OverlayService
{
    public interface IMessageBoxBuilder
    {
        public IMessageBoxBuilder WithMessage(string message);

        public IMessageBoxBuilder WithTitle(string title);

        public IMessageBoxBuilder WithHost(string hostId);

        public IMessageBoxBuilder WithIcon(MessageBoxIcon icon);

        public IMessageBoxBuilder WithButtons(MessageBoxButton buttons);

        public IMessageBoxBuilder WithTopLevelHash(int hash);

        public IMessageBoxBuilder WithStyleClass(string styleClass);

        public Task<MessageBoxResult> ShowAsync();
    }
}
