using Avalonia;

namespace TableConverter.ViewModels;

public partial class DataGenerationPageViewModel : BasePageViewModel
{
    public DataGenerationPageViewModel() : base("Data Generation", Application.Current?.Resources["DataIcon"])
    {
    }
}
