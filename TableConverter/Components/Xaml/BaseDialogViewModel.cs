using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;

namespace TableConverter.Components.Xaml;

public abstract partial class BaseDialogViewModel : ObservableObject
{
    protected readonly ISukiDialog dialog;

    public BaseDialogViewModel(ISukiDialog dialog)
    {
        this.dialog = dialog;
    }

    protected void Close() => dialog.Dismiss();
}
