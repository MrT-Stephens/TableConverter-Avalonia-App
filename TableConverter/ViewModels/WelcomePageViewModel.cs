using Avalonia;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace TableConverter.ViewModels;

public partial class WelcomePageViewModel : BasePageViewModel
{
    public WelcomePageViewModel(ISukiDialogManager dialogManager, ISukiToastManager toastManager) 
        : base(dialogManager, toastManager, "Welcome", Application.Current?.Resources["HandWaveIcon"])
    {
    }
}
