using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using TableConverter.Views.Controls;
using TableConverter.Views.Controls.Dialog;
using TableConverter.Views.Controls.Dialog.Options;

namespace TableConverter.Views;

public partial class MainView : BaseView
{
    public MainView()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        OverlayDialog.ShowModal(new TextBlock()
        {
            TextWrapping = TextWrapping.Wrap,
            Text =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
        }, null, null, new OverlayDialogOptions());
    }
}