namespace TableConverter.ViewModels;

public partial class WelcomePageViewModel : BasePageViewModel
{
    public WelcomePageViewModel() : base("Welcome", App.Current?.Resources["HandWaveIcon"])
    {
    }
}
