using TableConverter.ViewModels;

namespace TableConverter.Interfaces
{
    public interface IPane<TViewModel> where TViewModel : BaseViewModel
    {
        public string ID { get; }

        public string Header { get; }

        public TViewModel? ViewModel { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsVisible { get; set; }
    }
}
