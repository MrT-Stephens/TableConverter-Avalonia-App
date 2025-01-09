using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Dialogs;

namespace TableConverter.Components.Xaml;

public abstract class BaseDialogViewModel(ISukiDialog dialog) : ObservableObject
{
    protected void Close()
    {
        dialog.Dismiss();
    }
}