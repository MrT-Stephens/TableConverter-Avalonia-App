using Avalonia;

namespace TableConverter.ViewModels;

public partial class ConvertFilesPageViewModel : BasePageViewModel
{
    public ConvertFilesPageViewModel() : base("Convert Files", Application.Current?.Resources["ConvertIcon"])
    {
    }
}
