using Avalonia;

namespace TableConverter.ViewModels;

public partial class WelcomePageViewModel : BasePageViewModel
{
    public WelcomePageViewModel() : base("Welcome", Application.Current?.Resources["HandWaveIcon"])
    {
    }
}
