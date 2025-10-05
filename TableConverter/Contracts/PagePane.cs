using CommunityToolkit.Mvvm.ComponentModel;
using TableConverter.Interfaces;
using TableConverter.ViewModels;

namespace TableConverter.Contracts
{
    public partial class PagePane : ObservableObject, IPane<BasePageViewModel>
    {
        #region Properties

        [ObservableProperty] private string _ID;
        [ObservableProperty] private string _Header;
        [ObservableProperty] private BasePageViewModel? _ViewModel;
        [ObservableProperty] private bool _IsEnabled;
        [ObservableProperty] private bool _IsVisible;

        #endregion

        public PagePane(string id, string header, BasePageViewModel? viewModel = null, bool isEnabled = true, bool isVisible = true)
        {
            _ID = id;
            _Header = header;
            _ViewModel = viewModel;
            _IsEnabled = isEnabled;
            _IsVisible = isVisible;
        }
    }
}
