using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;

namespace TableConverter.Android;

[Activity(
    Label = "TableConverter",
    Name = "MrT_Stephens.TableConverter",
    Icon = "@drawable/icon",
    Theme = "@style/MyTheme.NoActionBar",
    AlwaysRetainTaskState = true,
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
}
